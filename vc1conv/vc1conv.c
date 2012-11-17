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

unsigned int process_chunk(unsigned int length, unsigned int stuffing);
unsigned int stuff_chunk(unsigned int length);
unsigned int strip_chunk(unsigned int length);
void putbits(unsigned int bits, unsigned int *ptr);
unsigned int getbits(unsigned int bits, unsigned int *ptr);

unsigned char	chunk_buffer[BUFFER_SIZE];
unsigned char	strip_buffer[BUFFER_SIZE];
unsigned char	process_buffer[BUFFER_SIZE];
unsigned char	output_buffer[BUFFER_SIZE];

unsigned int	getbitptr = 0;
unsigned int	putbitptr = 0;
unsigned int	coded_frames = 0;


typedef size_t (*StreamRead)(void* stream, void* buff, size_t length);
typedef size_t (*StreamWrite)(void* stream, void* buff, size_t length);

void TestLoad(void) {
	
}

size_t FileStreamRead(void* stream, void* buff, size_t length) {
	if(feof((FILE*)stream)) {
		return 0;
	}
	int ret = 0;
	while(ret == 0) {
		ret = fread(buff, 1, length, (FILE*)stream);
	}
	return ret;
}

size_t FileStreamWrite(void* stream, void* buff, size_t length) {
	return fwrite(buff, 1, length, (FILE*)stream);
}

int VC1ConvRemovePulldownStreamStream(StreamRead inStreamRead, void* inStream, StreamWrite outStreamWrite, void* outStream);

int VC1ConvRemovePulldownFileFile(const char* inFile, const char* outFile) {
	FILE	*fp, *fpout;
	
	/*--- open binary file (for parsing) ---*/
	fp = fopen(inFile, "rb");
	if (fp == 0) {
		fprintf(stderr, "Cannot open bitstream file <%s>\n", inFile);
		return 0;
	}

	/*--- open binary file (for parsing) ---*/
	fpout = fopen(outFile, "wb");
	if (fpout == 0) {
		fprintf(stderr, "Cannot open bitstream file <%s>\n", outFile);
		return 0;
	}
	
	int ret = VC1ConvRemovePulldownStreamStream(&FileStreamRead, fp, &FileStreamWrite, fpout);
	
	fclose(fp);
	fclose(fpout);
	return ret;
}

int VC1ConvRemovePulldownFileStream(const char* inFile, StreamWrite outStreamWrite, void* outStream) {
	FILE	*fp;
	
	/*--- open binary file (for parsing) ---*/
	fp = fopen(inFile, "rb");
	if (fp == 0) {
		fprintf(stderr, "Cannot open bitstream file <%s>\n", inFile);
		return 0;
	}
	
	int ret = VC1ConvRemovePulldownStreamStream(&FileStreamRead, fp, outStreamWrite, outStream);
	
	fclose(fp);
	return ret;
}

int VC1ConvRemovePulldownStreamFile(StreamRead inStreamRead, void* inStream, const char* outFile) {
	FILE	*fpout;
	
	/*--- open binary file (for parsing) ---*/
	fpout = fopen(outFile, "wb");
	if (fpout == 0) {
		fprintf(stderr, "Cannot open bitstream file <%s>\n", outFile);
		return 0;
	}
	
	int ret = VC1ConvRemovePulldownStreamStream(inStreamRead, inStream, &FileStreamWrite, fpout);
	
	fclose(fpout);
	return ret;
}

int VC1ConvRemovePulldownStreamStream(StreamRead inStreamRead, void* inStream, StreamWrite outStreamWrite, void* outStream)
{
	static unsigned char	input_buffer[BUFFER_SIZE];
	unsigned int	parse = 0xffffffff;
	unsigned int	xfer = FALSE;
	unsigned int	i, length, index, strip_length, stuff_length, process_length;
	unsigned int	first_sequence_input = FALSE;
	unsigned int	temp_time, hours, minutes;
	long double		time;

	getbitptr = 0;
	putbitptr = 0;
	coded_frames = 0;

	printf("vc1conv VC-1 Elementary Stream Converter 0.4\n");

	while((length = inStreamRead(inStream, &input_buffer[0], BUFFER_SIZE)) != 0)  {
		for(i = 0; i < length; i++)  {
			parse = (parse << 8) + input_buffer[i];
			if (parse == 0x0000010f)  {
				first_sequence_input = TRUE;
				if (xfer == FALSE)  {
					xfer = TRUE;
					index = 0;
					chunk_buffer[index++] = (parse >> 24) & 0xff;
					chunk_buffer[index++] = (parse >> 16) & 0xff;
					chunk_buffer[index++] = (parse >> 8) & 0xff;
				}
				else  {
					strip_length = strip_chunk(index - 3);
					strip_buffer[strip_length] = 0;
					process_length = process_chunk(strip_length, (index - 3 - strip_length));
					stuff_length = stuff_chunk(strip_length);
					outStreamWrite(outStream, &output_buffer[0], stuff_length);
					index = 0;
					chunk_buffer[index++] = (parse >> 24) & 0xff;
					chunk_buffer[index++] = (parse >> 16) & 0xff;
					chunk_buffer[index++] = (parse >> 8) & 0xff;
				}
			}
			else if (parse == 0x0000010d)  {
				if (first_sequence_input == TRUE)  {
					if (xfer == FALSE)  {
						xfer = TRUE;
						index = 0;
					}
					else  {
						strip_length = strip_chunk(index - 3);
						strip_buffer[strip_length] = 0;
						process_length = process_chunk(strip_length, (index - 3 - strip_length));
						stuff_length = stuff_chunk(strip_length);
						outStreamWrite(outStream, &output_buffer[0], stuff_length);
						index = 0;
						chunk_buffer[index++] = (parse >> 24) & 0xff;
						chunk_buffer[index++] = (parse >> 16) & 0xff;
						chunk_buffer[index++] = (parse >> 8) & 0xff;
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
						strip_length = strip_chunk(index - 3);
						strip_buffer[strip_length] = 0;
						process_length = process_chunk(strip_length, (index - 3 - strip_length));
						stuff_length = stuff_chunk(strip_length);
						outStreamWrite(outStream, &output_buffer[0], stuff_length);
						index = 0;
						chunk_buffer[index++] = (parse >> 24) & 0xff;
						chunk_buffer[index++] = (parse >> 16) & 0xff;
						chunk_buffer[index++] = (parse >> 8) & 0xff;
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
						strip_length = strip_chunk(index - 3);
						strip_buffer[strip_length] = 0;
						process_length = process_chunk(strip_length, (index - 3 - strip_length));
						stuff_length = stuff_chunk(strip_length);
						outStreamWrite(outStream, &output_buffer[0], stuff_length);
						index = 0;
						first_sequence_input = FALSE;
						xfer = FALSE;
					}
				}
			}
			if (xfer == TRUE)  {
				chunk_buffer[index++] = parse & 0xff;
				if (index > (BUFFER_SIZE))  {
					fprintf(stderr, "Picture bigger than 2 megabytes\n");
					exit(-1);
				}
			}
		}
	}
	time = 1.001/24.0 * (long double)coded_frames;
	temp_time = (long double)time;
	hours = temp_time / 3600;
	temp_time -= hours * 3600;
	minutes = temp_time / 60;
	temp_time -= minutes * 60;
	time -= (long double)((minutes * 60) + (hours * 3600));
	printf("\nframes = %d, running time = %d:%d:%f\n", coded_frames, hours, minutes, time);
	return 0;
}

unsigned int getbits(unsigned int bits, unsigned int *ptr)
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
		bits = 8 - (getbitptr % 8);
		if (bits == 8)  {
			bits = 0;
		}
	}
	index = getbitptr / 8;
	offset = getbitptr % 8;
	if (bits > 24)  {
		temp = (strip_buffer[index++] << 24) & 0xff000000;
		temp |= (strip_buffer[index++] << 16) & 0xff0000;
		temp |= (strip_buffer[index++] << 8) & 0xff00;
		temp |= strip_buffer[index++] & 0xff;
		if (offset != 0)  {
			temp = temp << offset;
			temp |= (strip_buffer[index++] >> (8 - offset)) & mask[offset];
		}
		temp = temp >> (32 - bits);
	}
	else if (bits > 16)  {
		temp = (strip_buffer[index++] << 16) & 0xff0000;
		temp |= (strip_buffer[index++] << 8) & 0xff00;
		temp |= strip_buffer[index++] & 0xff;
		if (offset != 0)  {
			temp = temp << offset;
			temp |= (strip_buffer[index++] >> (8 - offset)) & mask[offset];
		}
		temp = temp >> (24 - bits);
	}
	else if (bits > 8)  {
		temp = (strip_buffer[index++] << 8) & 0xff00;
		temp |= strip_buffer[index++] & 0xff;
		if (offset != 0)  {
			temp = temp << offset;
			temp |= (strip_buffer[index++] >> (8 - offset)) & mask[offset];
		}
		temp = temp >> (16 - bits);
	}
	else if (bits > 0)  {
		temp = strip_buffer[index++] & 0xff;
		if (offset != 0)  {
			temp = temp << offset;
			temp |= (strip_buffer[index++] >> (8 - offset)) & mask[offset];
		}
		temp = temp >> (8 - bits);
	}
	temp = temp & mask[bits];
	getbitptr = getbitptr + bits;
	*ptr = temp;
	return (bits);
}

void putbits(unsigned int bits, unsigned int *ptr)
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

	index = putbitptr / 8;
	offset = putbitptr % 8;
	total_bits = bits + offset;
	if (bits > 24)  {
		if (offset != 0)  {
			prev = process_buffer[index];
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
			process_buffer[index++] = (temp >> 32) & 0xff;
			process_buffer[index++] = (temp >> 24) & 0xff;
			process_buffer[index++] = (temp >> 16) & 0xff;
			process_buffer[index++] = (temp >> 8) & 0xff;
			process_buffer[index++] = temp & 0xff;
		}
		else  {
			process_buffer[index++] = (temp >> 24) & 0xff;
			process_buffer[index++] = (temp >> 16) & 0xff;
			process_buffer[index++] = (temp >> 8) & 0xff;
			process_buffer[index++] = temp & 0xff;
		}
	}
	else if (bits > 16)  {
		if (offset != 0)  {
			prev = process_buffer[index];
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
			process_buffer[index++] = (temp >> 24) & 0xff;
			process_buffer[index++] = (temp >> 16) & 0xff;
			process_buffer[index++] = (temp >> 8) & 0xff;
			process_buffer[index++] = temp & 0xff;
		}
		else  {
			process_buffer[index++] = (temp >> 16) & 0xff;
			process_buffer[index++] = (temp >> 8) & 0xff;
			process_buffer[index++] = temp & 0xff;
		}
	}
	else if (bits > 8)  {
		if (offset != 0)  {
			prev = process_buffer[index];
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
			process_buffer[index++] = (temp >> 16) & 0xff;
			process_buffer[index++] = (temp >> 8) & 0xff;
			process_buffer[index++] = temp & 0xff;
		}
		else  {
			process_buffer[index++] = (temp >> 8) & 0xff;
			process_buffer[index++] = temp & 0xff;
		}
	}
	else if (bits > 0)  {
		if (offset != 0)  {
			prev = process_buffer[index];
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
			process_buffer[index++] = (temp >> 8) & 0xff;
			process_buffer[index++] = temp & 0xff;
		}
		else  {
			process_buffer[index++] = temp;
		}
	}
	putbitptr = putbitptr + bits;
}

unsigned int strip_chunk(unsigned int length)
{
	unsigned int	i, parse = 0xffffffff;
	unsigned int	bits;
	unsigned int	index = 0;

	for (i = 0; i < length;)  {
		bits = chunk_buffer[i++];
		parse = (parse << 8) + bits;
		if ((parse & 0xffffff) == 0x000003)  {
			bits = chunk_buffer[i++];
			parse = (parse << 8) + bits;
			if (parse == 0x00000300 || parse == 0x00000301 || parse == 0x00000302 || parse == 0x00000303)  {
				strip_buffer[index++] = bits;
			}
			else  {
				strip_buffer[index++] = 0x3;
				strip_buffer[index++] = bits;
			}
		}
		else  {
			strip_buffer[index++] = bits;
		}
	}
	return (index);
}

unsigned int stuff_chunk(unsigned int length)
{
	unsigned int	i, parse = 0xffffffff;
	unsigned int	bits;
	unsigned int	index = 0;

	for (i = 0; i < length;)  {
		bits = process_buffer[i++];
		parse = (parse << 8) + bits;
		if ((parse & 0xffffff) == 0x000000)  {
			output_buffer[index++] = 0x3;
			parse = 0xffffff00;
		}
		else if ((parse & 0xffffff) == 0x000001)  {
			if (i > 3)  {
				output_buffer[index++] = 0x3;
			}
			parse = 0xffffff01;
		}
		else if ((parse & 0xffffff) == 0x000002)  {
			output_buffer[index++] = 0x3;
			parse = 0xffffff02;
		}
		else if ((parse & 0xffffff) == 0x000003)  {
			output_buffer[index++] = 0x3;
			parse = 0xffffff03;
		}
		output_buffer[index++] = bits;
	}
	return (index);
}

unsigned int process_chunk(unsigned int length, unsigned int stuffing)
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

	getbitptr = 0;
	putbitptr = 0;
	for (i = 0; i < length; i++)  {
		num = getbits(8, &bits);
		if (first_sequence == TRUE)  {
			putbits(8, &bits);
		}
		parse = (parse << 8) + bits;
		if (parse == 0x0000010d && i == 3)  {
			picture_count++;
			if (first_sequence == TRUE)  {
				coded_frames++;
			}
			if (interlace == 1)  {
				/* FCM */
				num = getbits(1, &fcm);
#ifdef CONVERT
				/* delete */
#else
				putbits(1, &fcm);
#endif
				if (fcm)  {
					num = getbits(1, &fcm);
#ifdef CONVERT
					/* delete */
#else
					putbits(1, &fcm);
#endif
					fcm = fcm | 0x2;
				}
			}
			/* PTYPE */
			num = getbits(1, &ptype);
			putbits(1, &ptype);
			if (ptype)  {
				num = getbits(1, &ptype);
				putbits(1, &ptype);
				if (ptype)  {
					num = getbits(1, &ptype);
					putbits(1, &ptype);
					if (ptype)  {
						num = getbits(1, &ptype);
						putbits(1, &ptype);
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
				num = getbits(8, &trash);
				putbits(8, &trash);
			}
#if 0
			printf("picture_type = %d\n", picture_type);
#endif
			if(pulldown == 1)  {
				/* RPTFRM, TFF, RFF */
				num = getbits(2, &temp_flags);
#ifdef CONVERT
				putbits(2, &zero);
#else
				putbits(2, &temp_flags);
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
				num = getbits(1, &ps);
				putbits(1, &ps);
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
					num = getbits(18, &trash);
					putbits(18, &trash);
					/* PS_VOFFSET */
					num = getbits(18, &trash);
					putbits(18, &trash);
					/* PS_WIDTH */
					num = getbits(14, &trash);
					putbits(14, &trash);
					/* PS_HEIGHT */
					num = getbits(14, &trash);
					putbits(14, &trash);
				}
			}
			if (picture_type != SKIPPED)  {
				/* RNDCTRL */
				num = getbits(1, &trash);
				putbits(1, &trash);
				if (interlace)  {
					/* UVSAMP */
					num = getbits(1, &trash);
#ifdef CONVERT
					/* delete */
#else
					putbits(1, &trash);
#endif
				}
				if (finter)  {
					/* INTRPFRM */
					num = getbits(1, &trash);
					putbits(1, &trash);
				}
				if (picture_type == B)  {
					/* BFRACTION */
					num = getbits(3, &trash);
					putbits(3, &trash);
					if (trash == 0x7)  {
						/* BFRACTION */
						num = getbits(4, &trash);
						putbits(4, &trash);
					}
				}
				/* PQINDEX */
				num = getbits(5, &pqindex);
				putbits(5, &pqindex);
				if (pqindex <= 8)  {
					/* HALFQP */
					num = getbits(1, &trash);
					putbits(1, &trash);
				}
				if (quantizer == 1)  {
					/* PQUANTIZER */
					num = getbits(1, &trash);
					putbits(1, &trash);
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
				putbits(8, &header);
				header = (parse >> 16) & 0xff;
				putbits(8, &header);
				header = (parse >> 8) & 0xff;
				putbits(8, &header);
				header = parse & 0xff;
				putbits(8, &header);
				first_sequence = TRUE;
			}
			/* PROFILE */
			num = getbits(2, &profile);
			putbits(2, &profile);
			/* LEVEL */
			num = getbits(3, &level);
			putbits(3, &level);
			/* COLORDIFF_FORMAT */
			num = getbits(2, &format);
			putbits(2, &format);
			/* FRMRTQ_POSTPROC, BITRTQ_POSTPROC */
			num = getbits(8, &trash);
			putbits(8, &trash);
			/* POSTPROCFLAG */
			num = getbits(1, &trash);
			putbits(1, &trash);
			/* MAX_CODED_WIDTH */
			num = getbits(12, &width);
			putbits(12, &width);
			/* MAX_CODED_HEIGHT */
			num = getbits(12, &height);
			putbits(12, &height);
			/* PULLDOWN */
			num = getbits(1, &pulldown);
			putbits(1, &pulldown);
			/* INTERLACE */
			num = getbits(1, &interlace);
#ifdef CONVERT
			putbits(1, &zero);
#else
			putbits(1, &interlace);
#endif
			/* TFCNTRFLAG */
			num = getbits(1, &tfcntrflag);
			putbits(1, &tfcntrflag);
			/* FINTERFLAG */
			num = getbits(1, &finter);
			putbits(1, &finter);
			/* RESERVED */
			num = getbits(1, &trash);
			putbits(1, &trash);
			/* PSF */
			num = getbits(1, &psf);
			putbits(1, &psf);
			/* DISPLAY_EXT */
			num = getbits(1, &display);
			putbits(1, &display);
			if (display == 1)  {
				/* DISPLAY_HORIZ_SIZE */
				num = getbits(14, &horz);
				putbits(14, &horz);
				/* DISPLAY_VERT_SIZE */
				num = getbits(14, &vert);
				putbits(14, &vert);
				/* ASPECT_RATIO_FLAG */
				num = getbits(1, &trash);
				putbits(1, &trash);
				if (trash == 1)  {
					/* ASPECT_RATIO */
					num = getbits(4, &aspect);
					putbits(4, &aspect);
					if (aspect == 15)  {
						/* ASPECT_HORIZ_SIZE, ASPECT_VERT_SIZE */
						num = getbits(16, &trash);
						putbits(16, &trash);
					}
				}
				else  {
					aspect = 0;
				}
				/* FRAMERATE_FLAG */
				num = getbits(1, &trash);
				putbits(1, &trash);
				if (trash == 1)  {
					/* FRAMERATEIND */
					num = getbits(1, &framerateind);
					putbits(1, &framerateind);
					if (framerateind == 0)  {
						/* FRAMERATENR */
						num = getbits(8, &frameratenr_int);
#ifdef CONVERT
						putbits(8, &one);
#else
						putbits(8, &frameratenr_int);
#endif
						/* FRAMERATEDR */
						num = getbits(4, &frameratedr_int);
						putbits(4, &frameratedr_int);
					}
					else  {
						/* FRAMERATEEXP */
						num = getbits(16, &framerateexp);
						putbits(16, &framerateexp);
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
			num = getbits(2, &trash);
			putbits(2, &trash);
			/* PANSCAN_FLAG */
			num = getbits(1, &panscan);
			putbits(1, &panscan);
			/* REFDIST_FLAG, LOOPFILTER, FASTUVMC, EXTENDED_MV, DQUANT, VTRANSFORM, OVERLAP */
			num = getbits(8, &trash);
			putbits(8, &trash);
			/* QUANTIZER */
			num = getbits(2, &quantizer);
			putbits(2, &quantizer);
			if (first == FALSE)  {
				entry_size = length + stuffing;
			}
		}
	}
	num = getbits(0, &trash);
	putbits(num, &trash);
	return (0);
}

/*
int main() {
	VC1ConvRemovePulldownFileFile("/home/john/Videos/Main_Movie_t01.vc1", "test.vc1");
	return 0;
}
*/