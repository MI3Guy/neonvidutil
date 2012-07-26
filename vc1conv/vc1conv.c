/*
VC-1 Elementary Stream converter
*/

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define	TRUE		1
#define	FALSE		0

#define I			0
#define P			1
#define B			2
#define BI			3
#define SKIPPED		4

#define	CONVERT

#define	BUFFER_SIZE		0x200000

typedef struct {
	unsigned char	chunk_buffer[BUFFER_SIZE];
	unsigned char	strip_buffer[BUFFER_SIZE];
	unsigned char	process_buffer[BUFFER_SIZE];
	unsigned char	output_buffer[BUFFER_SIZE];
	
	unsigned int	getbitptr;
	unsigned int	putbitptr;
	unsigned int	coded_frames;
} Globals;

unsigned int process_chunk(Globals* globals, unsigned int length, unsigned int stuffing);
unsigned int stuff_chunk(Globals* globals, unsigned int length);
unsigned int strip_chunk(Globals* globals, unsigned int length);
void putbits(Globals* globals, unsigned int bits, unsigned int *ptr);
unsigned int getbits(Globals* globals, unsigned int bits, unsigned int *ptr);

int VC1ConvRemovePulldown(const char* inFile, const char* outFile)
{
	Globals globals;
	
	FILE	*fp, *fpout;
	static unsigned char	input_buffer[BUFFER_SIZE];
	unsigned int	parse = 0xffffffff;
	unsigned int	xfer = FALSE;
	unsigned int	i, length, index, strip_length, stuff_length, process_length;
	unsigned int	first_sequence_input = FALSE;
	unsigned int	temp_time, hours, minutes;
	long double		time;
	
	globals.getbitptr = 0;
	globals.putbitptr = 0;
	globals.coded_frames = 0;

	/*--- open binary file (for parsing) ---*/
	fp = fopen(inFile, "rb");
	if (fp == 0) {
		fprintf(stderr, "Cannot open bitstream file <%s>\n", inFile);
		exit(-1);
	}

	/*--- open binary file (for parsing) ---*/
	fpout = fopen(outFile, "wb");
	if (fpout == 0) {
		fprintf(stderr, "Cannot open bitstream file <%s>\n", outFile);
		exit(-1);
	}

	printf("vc1conv VC-1 Elementary Stream Converter 0.4\n");

	while(!feof(fp))  {
		length = fread(&input_buffer[0], 1, (BUFFER_SIZE), fp);
		for(i = 0; i < length; i++)  {
			parse = (parse << 8) + input_buffer[i];
			if (parse == 0x0000010f)  {
				first_sequence_input = TRUE;
				if (xfer == FALSE)  {
					xfer = TRUE;
					index = 0;
					globals.chunk_buffer[index++] = (parse >> 24) & 0xff;
					globals.chunk_buffer[index++] = (parse >> 16) & 0xff;
					globals.chunk_buffer[index++] = (parse >> 8) & 0xff;
				}
				else  {
					strip_length = strip_chunk(&globals, index - 3);
					globals.strip_buffer[strip_length] = 0;
					process_length = process_chunk(&globals, strip_length, (index - 3 - strip_length));
					stuff_length = stuff_chunk(&globals, strip_length);
					fwrite(&globals.output_buffer[0], 1, stuff_length, fpout);
					index = 0;
					globals.chunk_buffer[index++] = (parse >> 24) & 0xff;
					globals.chunk_buffer[index++] = (parse >> 16) & 0xff;
					globals.chunk_buffer[index++] = (parse >> 8) & 0xff;
				}
			}
			else if (parse == 0x0000010d)  {
				if (first_sequence_input == TRUE)  {
					if (xfer == FALSE)  {
						xfer = TRUE;
						index = 0;
					}
					else  {
						strip_length = strip_chunk(&globals, index - 3);
						globals.strip_buffer[strip_length] = 0;
						process_length = process_chunk(&globals, strip_length, (index - 3 - strip_length));
						stuff_length = stuff_chunk(&globals, strip_length);
						fwrite(&globals.output_buffer[0], 1, stuff_length, fpout);
						index = 0;
						globals.chunk_buffer[index++] = (parse >> 24) & 0xff;
						globals.chunk_buffer[index++] = (parse >> 16) & 0xff;
						globals.chunk_buffer[index++] = (parse >> 8) & 0xff;
					}
				}
			}
			else if (parse == 0x0000010e)  {
				if (first_sequence_input == TRUE)  {
					if (xfer == FALSE)  {
						xfer = TRUE;
						index = 0;
					}
					else  {
						strip_length = strip_chunk(&globals, index - 3);
						globals.strip_buffer[strip_length] = 0;
						process_length = process_chunk(&globals, strip_length, (index - 3 - strip_length));
						stuff_length = stuff_chunk(&globals, strip_length);
						fwrite(&globals.output_buffer[0], 1, stuff_length, fpout);
						index = 0;
						globals.chunk_buffer[index++] = (parse >> 24) & 0xff;
						globals.chunk_buffer[index++] = (parse >> 16) & 0xff;
						globals.chunk_buffer[index++] = (parse >> 8) & 0xff;
					}
				}
			}
			else if (parse == 0x0000010a)  {
				if (first_sequence_input == TRUE)  {
					if (xfer == FALSE)  {
						xfer = TRUE;
						index = 0;
					}
					else  {
						strip_length = strip_chunk(&globals, index - 3);
						globals.strip_buffer[strip_length] = 0;
						process_length = process_chunk(&globals, strip_length, (index - 3 - strip_length));
						stuff_length = stuff_chunk(&globals, strip_length);
						fwrite(&globals.output_buffer[0], 1, stuff_length, fpout);
						index = 0;
						first_sequence_input = FALSE;
						xfer = FALSE;
					}
				}
			}
			if (xfer == TRUE)  {
				globals.chunk_buffer[index++] = parse & 0xff;
				if (index > (BUFFER_SIZE))  {
					fprintf(stderr, "Picture bigger than 2 megabytes\n");
					exit(-1);
				}
			}
		}
	}
	time = 1.001/24.0 * (long double)globals.coded_frames;
	temp_time = (long double)time;
	hours = temp_time / 3600;
	temp_time -= hours * 3600;
	minutes = temp_time / 60;
	temp_time -= minutes * 60;
	time -= (long double)((minutes * 60) + (hours * 3600));
	printf("\nframes = %d, running time = %d:%d:%f\n", globals.coded_frames, hours, minutes, time);
	fclose(fpout);
	fclose(fp);
	return 0;
}

unsigned int getbits(Globals* globals, unsigned int bits, unsigned int *ptr)
{
	static unsigned int	mask[33] = {0x0, 0x1, 0x3, 0x7, 0xf,
							0x1f, 0x3f, 0x7f, 0xff,
							0x1ff, 0x3ff, 0x7ff, 0xfff,
							0x1fff, 0x3fff, 0x7fff, 0xffff,
							0x1ffff, 0x3ffff, 0x7ffff, 0xfffff,
							0x1fffff, 0x3fffff, 0x7fffff, 0xffffff,
							0x1ffffff, 0x3ffffff, 0x7ffffff, 0xfffffff,
							0x1fffffff, 0x3fffffff, 0x7fffffff, 0xffffffff};
	unsigned int	temp, index, offset;

	if (bits == 0)  {
		bits = 8 - (globals->getbitptr % 8);
		if (bits == 8)  {
			bits = 0;
		}
	}
	index = globals->getbitptr / 8;
	offset = globals->getbitptr % 8;
	if (bits > 24)  {
		temp = (globals->strip_buffer[index++] << 24) & 0xff000000;
		temp |= (globals->strip_buffer[index++] << 16) & 0xff0000;
		temp |= (globals->strip_buffer[index++] << 8) & 0xff00;
		temp |= globals->strip_buffer[index++] & 0xff;
		if (offset != 0)  {
			temp = temp << offset;
			temp |= (globals->strip_buffer[index++] >> (8 - offset)) & mask[offset];
		}
		temp = temp >> (32 - bits);
	}
	else if (bits > 16)  {
		temp = (globals->strip_buffer[index++] << 16) & 0xff0000;
		temp |= (globals->strip_buffer[index++] << 8) & 0xff00;
		temp |= globals->strip_buffer[index++] & 0xff;
		if (offset != 0)  {
			temp = temp << offset;
			temp |= (globals->strip_buffer[index++] >> (8 - offset)) & mask[offset];
		}
		temp = temp >> (24 - bits);
	}
	else if (bits > 8)  {
		temp = (globals->strip_buffer[index++] << 8) & 0xff00;
		temp |= globals->strip_buffer[index++] & 0xff;
		if (offset != 0)  {
			temp = temp << offset;
			temp |= (globals->strip_buffer[index++] >> (8 - offset)) & mask[offset];
		}
		temp = temp >> (16 - bits);
	}
	else if (bits > 0)  {
		temp = globals->strip_buffer[index++] & 0xff;
		if (offset != 0)  {
			temp = temp << offset;
			temp |= (globals->strip_buffer[index++] >> (8 - offset)) & mask[offset];
		}
		temp = temp >> (8 - bits);
	}
	temp = temp & mask[bits];
	globals->getbitptr = globals->getbitptr + bits;
	*ptr = temp;
	return (bits);
}

void putbits(Globals* globals, unsigned int bits, unsigned int *ptr)
{
	static unsigned int	mask[33] = {0x0, 0x1, 0x3, 0x7, 0xf,
							0x1f, 0x3f, 0x7f, 0xff,
							0x1ff, 0x3ff, 0x7ff, 0xfff,
							0x1fff, 0x3fff, 0x7fff, 0xffff,
							0x1ffff, 0x3ffff, 0x7ffff, 0xfffff,
							0x1fffff, 0x3fffff, 0x7fffff, 0xffffff,
							0x1ffffff, 0x3ffffff, 0x7ffffff, 0xfffffff,
							0x1fffffff, 0x3fffffff, 0x7fffffff, 0xffffffff};
	unsigned int	temp, index, offset, prev, total_bits;

	index = globals->putbitptr / 8;
	offset = globals->putbitptr % 8;
	total_bits = bits + offset;
	if (bits > 24)  {
		if (offset != 0)  {
			prev = globals->process_buffer[index];
			if (total_bits > 32)  {
				temp = (*ptr << (40 - offset - bits)) | (prev << 32);
			}
			else  {
				temp = (*ptr << (32 - offset - bits)) | (prev << 24);
			}
		}
		else  {
			temp = *ptr << (32 - bits);
		}
		if (total_bits > 24)  {
			globals->process_buffer[index++] = (temp >> 32) & 0xff;
			globals->process_buffer[index++] = (temp >> 24) & 0xff;
			globals->process_buffer[index++] = (temp >> 16) & 0xff;
			globals->process_buffer[index++] = (temp >> 8) & 0xff;
			globals->process_buffer[index++] = temp & 0xff;
		}
		else  {
			globals->process_buffer[index++] = (temp >> 24) & 0xff;
			globals->process_buffer[index++] = (temp >> 16) & 0xff;
			globals->process_buffer[index++] = (temp >> 8) & 0xff;
			globals->process_buffer[index++] = temp & 0xff;
		}
	}
	else if (bits > 16)  {
		if (offset != 0)  {
			prev = globals->process_buffer[index];
			if (total_bits > 24)  {
				temp = (*ptr << (32 - offset - bits)) | (prev << 24);
			}
			else  {
				temp = (*ptr << (24 - offset - bits)) | (prev << 16);
			}
		}
		else  {
			temp = *ptr << (24 - bits);
		}
		if (total_bits > 24)  {
			globals->process_buffer[index++] = (temp >> 24) & 0xff;
			globals->process_buffer[index++] = (temp >> 16) & 0xff;
			globals->process_buffer[index++] = (temp >> 8) & 0xff;
			globals->process_buffer[index++] = temp & 0xff;
		}
		else  {
			globals->process_buffer[index++] = (temp >> 16) & 0xff;
			globals->process_buffer[index++] = (temp >> 8) & 0xff;
			globals->process_buffer[index++] = temp & 0xff;
		}
	}
	else if (bits > 8)  {
		if (offset != 0)  {
			prev = globals->process_buffer[index];
			if (total_bits > 16)  {
				temp = (*ptr << (24 - offset - bits)) | (prev << 16);
			}
			else  {
				temp = (*ptr << (16 - offset - bits)) | (prev << 8);
			}
		}
		else  {
			temp = *ptr << (16 - bits);
		}
		if (total_bits > 16)  {
			globals->process_buffer[index++] = (temp >> 16) & 0xff;
			globals->process_buffer[index++] = (temp >> 8) & 0xff;
			globals->process_buffer[index++] = temp & 0xff;
		}
		else  {
			globals->process_buffer[index++] = (temp >> 8) & 0xff;
			globals->process_buffer[index++] = temp & 0xff;
		}
	}
	else if (bits > 0)  {
		if (offset != 0)  {
			prev = globals->process_buffer[index];
			if (total_bits > 8)  {
				temp = (*ptr << (16 - offset - bits)) | (prev << 8);
			}
			else  {
				temp = (*ptr << (8 - offset - bits)) | prev;
			}
		}
		else  {
			temp = *ptr << (8 - bits);
		}
		if (total_bits > 8)  {
			globals->process_buffer[index++] = (temp >> 8) & 0xff;
			globals->process_buffer[index++] = temp & 0xff;
		}
		else  {
			globals->process_buffer[index++] = temp;
		}
	}
	globals->putbitptr = globals->putbitptr + bits;
}

unsigned int strip_chunk(Globals* globals, unsigned int length)
{
	unsigned int	i, parse = 0xffffffff;
	unsigned int	bits;
	unsigned int	index = 0;

	for (i = 0; i < length;)  {
		bits = globals->chunk_buffer[i++];
		parse = (parse << 8) + bits;
		if ((parse & 0xffffff) == 0x000003)  {
			bits = globals->chunk_buffer[i++];
			parse = (parse << 8) + bits;
			if (parse == 0x00000300 || parse == 0x00000301 || parse == 0x00000302 || parse == 0x00000303)  {
				globals->strip_buffer[index++] = bits;
			}
			else  {
				globals->strip_buffer[index++] = 0x3;
				globals->strip_buffer[index++] = bits;
			}
		}
		else  {
			globals->strip_buffer[index++] = bits;
		}
	}
	return (index);
}

unsigned int stuff_chunk(Globals* globals, unsigned int length)
{
	unsigned int	i, parse = 0xffffffff;
	unsigned int	bits;
	unsigned int	index = 0;

	for (i = 0; i < length;)  {
		bits = globals->process_buffer[i++];
		parse = (parse << 8) + bits;
		if ((parse & 0xffffff) == 0x000000)  {
			globals->output_buffer[index++] = 0x3;
			parse = 0xffffff00;
		}
		else if ((parse & 0xffffff) == 0x000001)  {
			if (i > 3)  {
				globals->output_buffer[index++] = 0x3;
			}
			parse = 0xffffff01;
		}
		else if ((parse & 0xffffff) == 0x000002)  {
			globals->output_buffer[index++] = 0x3;
			parse = 0xffffff02;
		}
		else if ((parse & 0xffffff) == 0x000003)  {
			globals->output_buffer[index++] = 0x3;
			parse = 0xffffff03;
		}
		globals->output_buffer[index++] = bits;
	}
	return (index);
}

unsigned int process_chunk(Globals* globals, unsigned int length, unsigned int stuffing)
{
	unsigned int	i, j;
	static unsigned int	parse = 0;
	static unsigned int	first = TRUE;
	static unsigned int	first_sequence = FALSE;
	static unsigned int	first_sequence_dump = FALSE;
	static unsigned long	sequence_size = 0;
	static unsigned long	entry_size = 0;
	static unsigned int	interlace, psf;
	static unsigned int	tfcntrflag;
	static unsigned int	ptype;
	static unsigned int	picture_type;
	static long double		frame_rate = 1.0;
	static long double		frameratenr;
	static long double		frameratedr;
	static unsigned int	picture_count = 0;
	static unsigned int	running_average_start = 0;
	static unsigned int	running_average_count = 0;
	static unsigned int	running_average_frames = 0;
	static unsigned int	running_average_samples[1024];
	static unsigned int	running_average_fields[1024];
	static unsigned int	video_fields = 0;
	static unsigned int	running_average_bitrate = 0;
	static unsigned int	temp_flags;
	long double		temp_running_average;
	long double		temp_running_fields;
	static unsigned int	fcm;
	static unsigned int	bits, num;
	static unsigned int	profile, level, format, width, height, pulldown, display, horz, vert;
	static unsigned int	aspect, framerateind, frameratenr_int, frameratedr_int, framerateexp;
	static unsigned int	trash, header, panscan, quantizer, ps, zero = 0;
	static unsigned int	windows, rff, rptfrm, finter, pqindex, one = 1;

	globals->getbitptr = 0;
	globals->putbitptr = 0;
	for (i = 0; i < length; i++)  {
		num = getbits(globals, 8, &bits);
		if (first_sequence == TRUE)  {
			putbits(globals, 8, &bits);
		}
		parse = (parse << 8) + bits;
		if (parse == 0x0000010d && i == 3)  {
			picture_count++;
			if (first_sequence == TRUE)  {
				globals->coded_frames++;
			}
			if (interlace == 1)  {
				/* FCM */
				num = getbits(globals, 1, &fcm);
#ifdef CONVERT
				/* delete */
#else
				putbits(1, &fcm);
#endif
				if (fcm)  {
					num = getbits(globals, 1, &fcm);
#ifdef CONVERT
					/* delete */
#else
					putbits(1, &fcm);
#endif
					fcm = fcm | 0x2;
				}
			}
			/* PTYPE */
			num = getbits(globals, 1, &ptype);
			putbits(globals, 1, &ptype);
			if (ptype)  {
				num = getbits(globals, 1, &ptype);
				putbits(globals, 1, &ptype);
				if (ptype)  {
					num = getbits(globals, 1, &ptype);
					putbits(globals, 1, &ptype);
					if (ptype)  {
						num = getbits(globals, 1, &ptype);
						putbits(globals, 1, &ptype);
						if (ptype)  {
							picture_type = SKIPPED;
						}
						else  {
							picture_type = BI;
						}
					}
					else  {
						picture_type = I;
					}
				}
				else  {
					picture_type = B;
				}
			}
			else  {
				picture_type = P;
			}
			if (tfcntrflag)  {
				/* TFCNTR */
				num = getbits(globals, 8, &trash);
				putbits(globals, 8, &trash);
			}
#if 0
			printf("picture_type = %d\n", picture_type);
#endif
			if(pulldown == 1)  {
				/* RPTFRM, TFF, RFF */
				num = getbits(globals, 2, &temp_flags);
#ifdef CONVERT
				putbits(globals, 2, &zero);
#else
				putbits(globals, 2, &temp_flags);
#endif
				if (interlace == 0 || psf == 1)  {
					rptfrm = temp_flags;
				}
				else  {
					rff = temp_flags & 0x1;
				}
			}
			if (panscan)  {
				/* PS_PRESENT */
				num = getbits(globals, 1, &ps);
				putbits(globals, 1, &ps);
				if (ps)  {
					if (interlace == 1 && psf == 0)  {
						if (pulldown == 1)  {
							windows = 2 + rff;
						}
						else  {
							windows = 2;
						}
					}
					else {
						if (pulldown == 1)  {
							windows = 1 + rptfrm;
						}
						else  {
							windows = 1;
						}
					}
				}
				for (i = 0; i < windows; i++)  {
					/* PS_HOFFSET */
					num = getbits(globals, 18, &trash);
					putbits(globals, 18, &trash);
					/* PS_VOFFSET */
					num = getbits(globals, 18, &trash);
					putbits(globals, 18, &trash);
					/* PS_WIDTH */
					num = getbits(globals, 14, &trash);
					putbits(globals, 14, &trash);
					/* PS_HEIGHT */
					num = getbits(globals, 14, &trash);
					putbits(globals, 14, &trash);
				}
			}
			if (picture_type != SKIPPED)  {
				/* RNDCTRL */
				num = getbits(globals, 1, &trash);
				putbits(globals, 1, &trash);
				if (interlace)  {
					/* UVSAMP */
					num = getbits(globals, 1, &trash);
#ifdef CONVERT
					/* delete */
#else
					putbits(1, &trash);
#endif
				}
				if (finter)  {
					/* INTRPFRM */
					num = getbits(globals, 1, &trash);
					putbits(globals, 1, &trash);
				}
				if (picture_type == B)  {
					/* BFRACTION */
					num = getbits(globals, 3, &trash);
					putbits(globals, 3, &trash);
					if (trash == 0x7)  {
						/* BFRACTION */
						num = getbits(globals, 4, &trash);
						putbits(globals, 4, &trash);
					}
				}
				/* PQINDEX */
				num = getbits(globals, 5, &pqindex);
				putbits(globals, 5, &pqindex);
				if (pqindex <= 8)  {
					/* HALFQP */
					num = getbits(globals, 1, &trash);
					putbits(globals, 1, &trash);
				}
				if (quantizer == 1)  {
					/* PQUANTIZER */
					num = getbits(globals, 1, &trash);
					putbits(globals, 1, &trash);
				}
			}
			if(interlace == 1)  {
				if(temp_flags & 0x1)  {
					video_fields += 3;
					running_average_fields[running_average_frames] = 3;
				}
				else  {
					video_fields += 2;
					running_average_fields[running_average_frames] = 2;
				}
			}
			else  {
				switch(temp_flags & 0x3)  {
					case 3:
						video_fields += 4;
						running_average_fields[running_average_frames] = 4;
						break;
					case 2:
						video_fields += 3;
						running_average_fields[running_average_frames] = 3;
						break;
					case 1:
						video_fields += 2;
						running_average_fields[running_average_frames] = 2;
						break;
					case 0:
						video_fields += 1;
						running_average_fields[running_average_frames] = 1;
						break;
					default:
						video_fields += 0;
						break;
				}
			}
			if(first == TRUE)  {
				first = FALSE;
			}
#if 0
			printf("%8ld\n", (length + stuffing + sequence_size + entry_size) * 8);
#endif
			running_average_samples[running_average_frames] = (length + stuffing + sequence_size + entry_size) * 8;
			sequence_size = 0;
			entry_size = 0;
			running_average_frames = (running_average_frames + 1) & 1023;
			running_average_count++;
			if(running_average_count == 300)  {
				running_average_count = 299;
				temp_running_average = 0;
				temp_running_fields = 0.0;
				for(j = 0; j < 300; j++)  {
					temp_running_average += running_average_samples[(running_average_start + j) & 1023];
					temp_running_fields += running_average_fields[(running_average_start + j) & 1023];
				}
				running_average_start = (running_average_start + 1) & 1023;
				if(interlace == 1)  {
					running_average_bitrate = (unsigned int)((temp_running_average / 300.0) * ((600.0 / temp_running_fields) * frame_rate));
				}
				else  {
					running_average_bitrate = (unsigned int)((temp_running_average / 300.0) * ((300.0 / temp_running_fields) * frame_rate));
				}
				printf("bitrate = %9d\r", running_average_bitrate);
			}
		}
		else if (parse == 0x0000010f && i == 3)  {
			if (first_sequence_dump == FALSE)  {
				printf("Sequence Header found\n");
			}
			if (first_sequence == FALSE)  {
				printf("%d frames before first I-frame\n", picture_count);
				header = (parse >> 24) & 0xff;
				putbits(globals, 8, &header);
				header = (parse >> 16) & 0xff;
				putbits(globals, 8, &header);
				header = (parse >> 8) & 0xff;
				putbits(globals, 8, &header);
				header = parse & 0xff;
				putbits(globals, 8, &header);
				first_sequence = TRUE;
			}
			/* PROFILE */
			num = getbits(globals, 2, &profile);
			putbits(globals, 2, &profile);
			/* LEVEL */
			num = getbits(globals, 3, &level);
			putbits(globals, 3, &level);
			/* COLORDIFF_FORMAT */
			num = getbits(globals, 2, &format);
			putbits(globals, 2, &format);
			/* FRMRTQ_POSTPROC, BITRTQ_POSTPROC */
			num = getbits(globals, 8, &trash);
			putbits(globals, 8, &trash);
			/* POSTPROCFLAG */
			num = getbits(globals, 1, &trash);
			putbits(globals, 1, &trash);
			/* MAX_CODED_WIDTH */
			num = getbits(globals, 12, &width);
			putbits(globals, 12, &width);
			/* MAX_CODED_HEIGHT */
			num = getbits(globals, 12, &height);
			putbits(globals, 12, &height);
			/* PULLDOWN */
			num = getbits(globals, 1, &pulldown);
			putbits(globals, 1, &pulldown);
			/* INTERLACE */
			num = getbits(globals, 1, &interlace);
#ifdef CONVERT
			putbits(globals, 1, &zero);
#else
			putbits(globals, 1, &interlace);
#endif
			/* TFCNTRFLAG */
			num = getbits(globals, 1, &tfcntrflag);
			putbits(globals, 1, &tfcntrflag);
			/* FINTERFLAG */
			num = getbits(globals, 1, &finter);
			putbits(globals, 1, &finter);
			/* RESERVED */
			num = getbits(globals, 1, &trash);
			putbits(globals, 1, &trash);
			/* PSF */
			num = getbits(globals, 1, &psf);
			putbits(globals, 1, &psf);
			/* DISPLAY_EXT */
			num = getbits(globals, 1, &display);
			putbits(globals, 1, &display);
			if (display == 1)  {
				/* DISPLAY_HORIZ_SIZE */
				num = getbits(globals, 14, &horz);
				putbits(globals, 14, &horz);
				/* DISPLAY_VERT_SIZE */
				num = getbits(globals, 14, &vert);
				putbits(globals, 14, &vert);
				/* ASPECT_RATIO_FLAG */
				num = getbits(globals, 1, &trash);
				putbits(globals, 1, &trash);
				if (trash == 1)  {
					/* ASPECT_RATIO */
					num = getbits(globals, 4, &aspect);
					putbits(globals, 4, &aspect);
					if (aspect == 15)  {
						/* ASPECT_HORIZ_SIZE, ASPECT_VERT_SIZE */
						num = getbits(globals, 16, &trash);
						putbits(globals, 16, &trash);
					}
				}
				else  {
					aspect = 0;
				}
				/* FRAMERATE_FLAG */
				num = getbits(globals, 1, &trash);
				putbits(globals, 1, &trash);
				if (trash == 1)  {
					/* FRAMERATEIND */
					num = getbits(globals, 1, &framerateind);
					putbits(globals, 1, &framerateind);
					if (framerateind == 0)  {
						/* FRAMERATENR */
						num = getbits(globals, 8, &frameratenr_int);
#ifdef CONVERT
						putbits(globals, 8, &one);
#else
						putbits(globals, 8, &frameratenr_int);
#endif
						/* FRAMERATEDR */
						num = getbits(globals, 4, &frameratedr_int);
						putbits(globals, 4, &frameratedr_int);
					}
					else  {
						/* FRAMERATEEXP */
						num = getbits(globals, 16, &framerateexp);
						putbits(globals, 16, &framerateexp);
					}
				}
			}
			if (first_sequence_dump == FALSE)  {
				if (profile == 3)  {
					printf("Advanced Profile\n");
				}
				else  {
					printf("Reserved Profile\n");
				}
				if (level > 4)  {
					printf("Level = Reserved\n");
				}
				else  {
					printf("Level = %d\n", level);
				}
				if (format == 1)  {
					printf("Chroma Format = 4:2:0\n");
				}
				else  {
					printf("Chroma Format = Reserved\n");
				}
				printf("Horizontal size = %d\n", ((width * 2) + 2));
				printf("Vertical size = %d\n", ((height * 2) + 2));
				printf("Pulldown = %d\n", pulldown);
				printf("Interlace = %d\n", interlace);
				if (display)  {
					printf("Display Horizontal size = %d\n", horz + 1);
					printf("Display Vertical size = %d\n", vert + 1);
				}
				switch (aspect & 0xf)  {
					case 0:
						printf("Aspect ratio = unspecified\n");
						break;
					case 1:
						printf("Aspect ratio = 1:1 (square samples)\n");
						break;
					case 2:
						printf("Aspect ratio = 12:11 (704x576 4:3)\n");
						break;
					case 3:
						printf("Aspect ratio = 10:11 (704x480 4:3)\n");
						break;
					case 4:
						printf("Aspect ratio = 16:11 (704x576 16:9)\n");
						break;
					case 5:
						printf("Aspect ratio = 40:33 (704x480 16:9)\n");
						break;
					case 6:
						printf("Aspect ratio = 24:11 (352x576 4:3)\n");
						break;
					case 7:
						printf("Aspect ratio = 20:11 (352x480 4:3)\n");
						break;
					case 8:
						printf("Aspect ratio = 32:11 (352x576 16:9)\n");
						break;
					case 9:
						printf("Aspect ratio = 80:33 (352x480 16:9)\n");
						break;
					case 10:
						printf("Aspect ratio = 18:11 (480x576 4:3)\n");
						break;
					case 11:
						printf("Aspect ratio = 15:11 (480x480 4:3)\n");
						break;
					case 12:
						printf("Aspect ratio = 64:33 (528x576 16:9)\n");
						break;
					case 13:
						printf("Aspect ratio = 160:99 (528x480 16:9)\n");
						break;
					case 14:
						printf("Aspect ratio = Reserved\n");
						break;
					case 15:
						break;
				}
				if (framerateind == 0)  {
					switch (frameratenr_int)  {
						case 0:
							printf("Forbidden Frame Rate!\n");
							break;
						case 1:
							frameratenr = 24000.0;
							break;
						case 2:
							frameratenr = 25000.0;
							break;
						case 3:
							frameratenr = 30000.0;
							break;
						case 4:
							frameratenr = 50000.0;
							break;
						case 5:
							frameratenr = 60000.0;
							break;
						case 6:
							frameratenr = 48000.0;
							break;
						case 7:
							frameratenr = 72000.0;
							break;
						default:
							printf("Reserved Frame Rate!\n");
							break;
					}
					switch (frameratedr_int)  {
						case 0:
							printf("Forbidden Frame Rate!\n");
							break;
						case 1:
							frameratedr = 1000.0;
							break;
						case 2:
							frameratedr = 1001.0;
							break;
						default:
							printf("Reserved Frame Rate!\n");
							break;
					}
					frame_rate = frameratenr / frameratedr;
					printf("Frame Rate = %.3f\n", frame_rate);
				}
				else  {
					frame_rate = ((long double)(framerateexp + 1)) / 32.0;
					printf("Frame Rate = %.3f\n", frame_rate);
				}
				first_sequence_dump = TRUE;
			}
			if (first == FALSE)  {
				sequence_size = length + stuffing;
			}
		}
		else if (parse == 0x0000010e && i == 3)  {
			/* BROKEN LINK, CLOSED_ENTRY */
			num = getbits(globals, 2, &trash);
			putbits(globals, 2, &trash);
			/* PANSCAN_FLAG */
			num = getbits(globals, 1, &panscan);
			putbits(globals, 1, &panscan);
			/* REFDIST_FLAG, LOOPFILTER, FASTUVMC, EXTENDED_MV, DQUANT, VTRANSFORM, OVERLAP */
			num = getbits(globals, 8, &trash);
			putbits(globals, 8, &trash);
			/* QUANTIZER */
			num = getbits(globals, 2, &quantizer);
			putbits(globals, 2, &quantizer);
			if (first == FALSE)  {
				entry_size = length + stuffing;
			}
		}
	}
	num = getbits(globals, 0, &trash);
	putbits(globals, num, &trash);
	return (0);
}
