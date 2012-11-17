#include "DGPulldown.h"

/* 
 *  DGPulldown Copyright (C) 2005-2007, Donald Graft
 *
 *  DGPulldown is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  DGPulldown is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 */

/*------
Version 1.0.11: Fixed the broken drop frame option.
Version 1.0.10: Fixed a bug in in-place operation that corrupted the last
				32Kbytes of the file.
Version 1.0.9 : Fixed a bug in the initialization of the destination file
				edit box when drag-and-drop is used.
Version 1.0.8 : Fixed 2GB limit problem of in-place operation,
				made TFF/BFF selction user configurable
				(because the previous automatic stream reading was broken),
				and added a GUI configurable output path.
				Changes by 'neuron2'.
Version 1.0.7 : Added option for in-place output (input file is modified).
				Changes by 'timecop'.
Version 1.0.6 : Added CLI interface.
				Changes by 'neuron2'/'timecop'.
Version 1.0.5 : Added drag-and-drop to input file edit box.
                Changes by 'timecop'.
Version 1.0.4 : Repaired broken source frame rate edit box
                (some valid input rates are rejected).
				Changes by 'neuron2'.
------*/
#define _LARGEFILE64_SOURCE

//#include <windows.h>
//#include "resource.h"
#include <stdio.h> 
#ifdef WINDOWS
#include <io.h>
#else
#include <unistd.h>
#include <sys/types.h>
#include <sys/stat.h>
#endif
#include <fcntl.h>
//#include <commctrl.h>
#include <math.h>
#include <inttypes.h>
#include <cstring>
#include <iostream>

bool check_options(void);

//char input_file[2048];
//char output_file[2048];
#define ES 0
#define PROGRAM 1
int stream_type;

#define CONVERT_NO_CHANGE       0
#define CONVERT_23976_TO_29970  1
#define CONVERT_24000_TO_29970  2
#define CONVERT_25000_TO_29970  3
#define CONVERT_CUSTOM          4

#ifndef WIN32
#define O_BINARY 0
#endif

void process();

//unsigned int CliActive = 0;
unsigned int Rate = CONVERT_23976_TO_29970;
char InputRate[255];
char OutputRate[255];
unsigned int TimeCodes = 1;
unsigned int DropFrames = 2;
unsigned int StartTime = 0;
char HH[255] = {0}, MM[255] = {0}, SS[255] = {0}, FF[255] = {0};
//unsigned int Debug = 0;
int tff = 1;
static int output_m2v = 1;

// Used for exiting the processing.
class DGPFinishedException {};
class DGPErrorException {};

// Defines for the start code detection state machine.
#define NEED_FIRST_0  0
#define NEED_SECOND_0 1
#define NEED_1        2

typedef int (*IOReadFunction)(void* buff, int count);
typedef void (*IOWriteFunction)(void* buff, int count);
typedef void (*IOResetFunction)();

FILE *fp = NULL;
FILE *wfp = NULL;
IOReadFunction readFunc = NULL;
IOWriteFunction writeFunc = NULL;
IOResetFunction resetFunc = NULL;

#define BUFFER_SIZE 32768
unsigned char inbuffer[BUFFER_SIZE];
unsigned char outbuffer[BUFFER_SIZE];
int inBufferIndex;
int inBufferSize;
int outBufferIndex;

void determine_stream_type(void);
void video_parser(void);
void pack_parser(void);
void generate_flags(void);
void flush_output();

#define MAX_PATTERN_LENGTH 2000000
unsigned char bff_flags[MAX_PATTERN_LENGTH];
unsigned char tff_flags[MAX_PATTERN_LENGTH];

static int state, found;
static int f, F, D;
static int ref;
int EOF_reached;
float tfps, cfps;
int64_t current_num, current_den, target_num, target_den;
int rate;
int rounded_fps;

int field_count, pict, sec, minute, hour, drop_frame;
int set_tc;


extern "C" {
	void TestLoad() {
		
	}
	
	bool DGPulldownRemoveFileFile(const char* inFile, const char* outFile) {
		Rate = CONVERT_CUSTOM;
		strcpy(InputRate, "23.976");
		strcpy(OutputRate, "23.976");
		
		readFunc = NULL;
		writeFunc = NULL;
		resetFunc = NULL;
		
		fp = fopen(inFile, "rb");
		wfp = fopen(outFile, "wb");
		
		try {
			process();
		}
		catch(DGPFinishedException&) {
			
		}
		catch(DGPErrorException&) {
			std::cerr << "An error occurred. Exiting\n";
			fclose(fp);
			fclose(wfp);
			
			return false;
		}
		
		fclose(fp);
		fclose(wfp);
		
		
		return true;
	}
	
	bool DGPulldownRemoveFileStream(const char* inFile, IOWriteFunction write) {
		Rate = CONVERT_CUSTOM;
		strcpy(InputRate, "23.976");
		strcpy(OutputRate, "23.976");
		
		readFunc = NULL;
		resetFunc = NULL;
		wfp = NULL;
		
		fp = fopen(inFile, "rb");
		writeFunc = write;
		
		try {
			process();
		}
		catch(DGPFinishedException&) {
			
		}
		catch(DGPErrorException&) {
			std::cerr << "An error occurred. Exiting\n";
			return false;
		}
		
		fclose(fp);
		
		return true;
	}
	
	bool DGPulldownRemoveStreamFile(IOReadFunction read, IOResetFunction reset, const char* outFile) {
		Rate = CONVERT_CUSTOM;
		strcpy(InputRate, "23.976");
		strcpy(OutputRate, "23.976");
		
		fp = NULL;
		writeFunc = NULL;
		
		readFunc = read;
		resetFunc = reset;
		wfp = fopen(outFile, "wb");
		
		try {
			process();
		}
		catch(DGPFinishedException&) {
			
		}
		catch(DGPErrorException&) {
			std::cerr << "An error occurred. Exiting\n";
			fclose(fp);
			fclose(wfp);
			
			return false;
		}
		
		fclose(fp);
		fclose(wfp);
		
		
		return true;
	}
	
	bool DGPulldownRemoveStreamStream(IOReadFunction read, IOResetFunction reset, IOWriteFunction write) {
		Rate = CONVERT_CUSTOM;
		strcpy(InputRate, "23.976");
		strcpy(OutputRate, "23.976");
		
		fp = NULL;
		wfp = NULL;
		
		readFunc = read;
		writeFunc = write;
		resetFunc = reset;
		
		try {
			process();
		}
		catch(DGPFinishedException&) {
			
		}
		catch(DGPErrorException&) {
			std::cerr << "An error occurred. Exiting\n";
			return false;
		}
		
		return true;
	}
}





void KillThread(bool ok)
{
	fprintf(stderr, "Done.\n");
	flush_output();
	if(ok) {
		throw DGPFinishedException();
	}
	else {
		throw DGPErrorException();
	}
}

// Write a value in the file at an offset relative
// to the last read character. This allows for
// read ahead (write behind) by using negative offsets.
// Currently, it's only used for the 4-byte timecode.
// The offset is ignored when not doing in-place operation.
inline void put_byte(int offset, unsigned char val)
{
	if(wfp != NULL) {
		fwrite(&val, 1, 1, wfp);
	}
	else {
		outbuffer[outBufferIndex] = val;
		++outBufferIndex;
		if(outBufferIndex == BUFFER_SIZE) {
			writeFunc(outbuffer, BUFFER_SIZE);
			outBufferIndex = 0;
		}
	}
}

void flush_output() {
	if(wfp != NULL) {
		fflush(wfp);
	}
	else {
		writeFunc(outbuffer, outBufferIndex);
		outBufferIndex = 0;
	}
}

// Get a byte from the stream. We do our own buffering
// for in-place operation, otherwise we rely on the
// operating system.
inline unsigned char get_byte(void)
{
	unsigned char val;

	if(fp != NULL) {
		if (fread(&val, 1, 1, fp) != 1)
		{
			flush_output();
			KillThread(true);
		}
	}
	else {
		if(inBufferIndex < inBufferSize) {
			val = inbuffer[inBufferIndex];
			++inBufferIndex;
		}
		else {
			inBufferIndex = 1;
			inBufferSize = readFunc(inbuffer, BUFFER_SIZE);
			if(inBufferSize == 0) {
				flush_output();
				KillThread(true);
			}
			val = inbuffer[0];
		}
	}
	return val;
}

inline void reset_input() {
	if(fp != NULL) {
		fseek(fp, 0, SEEK_SET);
	}
	else {
		resetFunc();
		inBufferIndex = 0;
		inBufferSize = 0;
	}
}

void process()
{
	F = 0;
	field_count = 0;
	pict = 0;
	sec = 0;
	minute = 0;
	hour = 0;
	drop_frame = 0;

	// Determine the stream type: ES or program.
	determine_stream_type();
	if (stream_type == PROGRAM)
	{
		printf("The input file must be an elementary\nstream, not a program stream.\n");
		KillThread(false);
	}

	reset_input();

	// Make sure all the options are ok
	if (!check_options()) 
	{
		KillThread(false);
	}

	// Generate the flags sequences.
	// It's easier and cleaner to pre-calculate than
	// to try to work it out on the fly in encode order.
	// Pre-calculation can work in display order.
	generate_flags();

	// Start converting.
	video_parser();
}

void video_parser(void)
{
	unsigned char val, tc[4];
	int trf;
	
	// Inits.
	state = NEED_FIRST_0;
	found = 0;
	f = F = 0;
	EOF_reached = 0;

	// Let's go!
	while(1)
	{
		// Parse for start codes.
		val = get_byte();

	    if (output_m2v) put_byte(0, val);

		switch (state)
		{
		case NEED_FIRST_0:
			if (val == 0)
				state = NEED_SECOND_0;
			break;
		case NEED_SECOND_0:
			if (val == 0)
				state = NEED_1;
			else
				state = NEED_FIRST_0;
			break;
		case NEED_1:
			if (val == 1)
			{
				found = 1;
				state = NEED_FIRST_0;
			}
			else if (val != 0)
				state = NEED_FIRST_0;
			break;
		}
		if (found == 1)
		{
			// Found a start code.
			found = 0;
			val = get_byte();
			if (output_m2v) put_byte(0, val);

			if (val == 0xb8)
			{
				// GOP.
				F += f;
				f = 0;
				if (set_tc)
				{
					// Timecode and drop frame
					for(pict=0; pict<4; pict++) 
						tc[pict] = get_byte();

					//determine frame->tc
					pict = field_count >> 1;
					if (drop_frame) pict += 18*(pict/17982) + 2*((pict%17982 - 2) / 1798);
					pict -= (sec = pict / rounded_fps) * rounded_fps; 
					sec -= (minute = sec / 60) * 60; 
					minute -= (hour = minute / 60) * 60;
					hour %= 24;
					//now write timecode
					val = drop_frame | (hour << 2) | ((minute & 0x30) >> 4);
					if (output_m2v) put_byte(-3, val);
					val = ((minute & 0x0f) << 4) | 0x8 | ((sec & 0x38) >> 3);
					if (output_m2v) put_byte(-2, val);
					val = ((sec & 0x07) << 5) | ((pict & 0x1e) >> 1);
					if (output_m2v) put_byte(-1, val);
					val = (tc[3] & 0x7f) | ((pict & 0x1) << 7);
					if (output_m2v) put_byte(0, val);
				}
				else
				{
					//just read timecode
					val = get_byte();
					if (output_m2v) put_byte(0, val);
					drop_frame = (val & 0x80) >> 7;
					minute = (val & 0x03) << 4;
					hour = (val & 0x7c) >> 2;
					val = get_byte();
					if (output_m2v) put_byte(0, val);
					minute |= (val & 0xf0) >> 4;
					sec = (val & 0x07) << 3;
					val = get_byte();
					if (output_m2v) put_byte(0, val);
					sec |= (val & 0xe0) >> 5;
					pict = (val & 0x1f) << 1;
					val = get_byte();
					if (output_m2v) put_byte(0, val);
					pict |= (val & 0x80) >> 7;
				}
			}
			else if (val == 0x00)
			{
				// Picture.
				val = get_byte();
				if (output_m2v) put_byte(0, val);
				ref = (val << 2);
				val = get_byte();
				if (output_m2v) put_byte(0, val);
				ref |= (val >> 6);
				D = F + ref;
				f++;
				if (D >= MAX_PATTERN_LENGTH - 1)
				{
					printf("Maximum filelength exceeded, aborting!\n");
					KillThread(false);
				}
			}
			else if ((rate != -1) && (val == 0xB3))
			{
				// Sequence header.
				val = get_byte();
				if (output_m2v) put_byte(0, val);
				val = get_byte();
				if (output_m2v) put_byte(0, val);
				val = get_byte();
				if (output_m2v) put_byte(0, val);
				val = (get_byte() & 0xf0) | rate;
				if (output_m2v) put_byte(0, val);
			}
			else if (val == 0xB5)
			{
				val = get_byte();
				if (output_m2v) put_byte(0, val);
				if ((val & 0xf0) == 0x80)
				{
					// Picture coding extension.
					val = get_byte();
					if (output_m2v) put_byte(0, val);
					val = get_byte();
					if (output_m2v) put_byte(0, val);
					val = get_byte();
					//rewrite trf
					trf = tff ? tff_flags[D] : bff_flags[D];
					val &= 0x7d;
					val |= (trf & 2) << 6;
					val |= (trf & 1) << 1;
					field_count += 2 + (trf & 1);
					if (output_m2v) put_byte(0, val);
					// Set progressive frame. This is needed for RFFs to work.
					val = get_byte() | 0x80;
					if (output_m2v) put_byte(0, val);
				}
				else if ((val & 0xf0) == 0x10)
				{
					// Sequence extension
					// Clear progressive sequence. This is needed to
					// get RFFs to work field-based.
					val = get_byte() & ~0x08;
					if (output_m2v) put_byte(0, val);
				}
			}
		}
	}
	return;
}

void determine_stream_type(void)
{
	//int i;
	unsigned char val, tc[4];
	int state = 0, found = 0;

	stream_type = ES;

	// Look for start codes.
	state = NEED_FIRST_0;
	// Read timecode, and rate from stream
	field_count = -1;
	rate = -1;
	while ((field_count==-1) || (rate==-1))
	{
		val = get_byte();
		switch (state)
		{
		case NEED_FIRST_0:
			if (val == 0)
				state = NEED_SECOND_0;
			break;
		case NEED_SECOND_0:
			if (val == 0)
				state = NEED_1;
			else
				state = NEED_FIRST_0;
			break;
		case NEED_1:
			if (val == 1)
			{
				found = 1;
				state = NEED_FIRST_0;
			}
			else if (val != 0)
				state = NEED_FIRST_0;
			break;
		}
		if (found == 1)
		{
			// Found a start code.
			found = 0;
			val = get_byte();
			if (val == 0xba)
			{
				stream_type = PROGRAM;
				break;
			}
			else if (val == 0xb8)
			{
				// GOP.
				if (field_count == -1) {
					for(pict=0; pict<4; pict++) tc[pict] = get_byte();
					drop_frame = (tc[0] & 0x80) >> 7;
					hour = (tc[0] & 0x7c) >> 2;
					minute = (tc[0] & 0x03) << 4 | (tc[1] & 0xf0) >> 4;
					sec = (tc[1] & 0x07) << 3 | (tc[2] & 0xe0) >> 5;
					pict = (tc[2] & 0x1f) << 1 | (tc[3] & 0x80) >> 7;
					field_count = -2;
				}
			}
			else if (val == 0xB3)
			{
				// Sequence header.
				if (rate == -1) {
					get_byte();
					get_byte();
					get_byte();
					rate = get_byte() & 0x0f;
				}
			}
		}
	}
}

bool check_options(void)
{
    char buf[100];

	float float_rates[9] = { 0.0, (float)23.976, 24.0, 25.0, (float)29.97, 30.0, 50.0, (float)59.94, 60.0 };
	
	if (Rate == CONVERT_NO_CHANGE)
	{
		tfps = float_rates[rate];
	}
	else if (Rate == CONVERT_23976_TO_29970)
	{
		tfps = (float) 29.970;
		cfps = (float) 23.976;
		current_num = 24000;
		current_den = 1001;
	}
	else if (Rate == CONVERT_24000_TO_29970)
	{
		tfps = (float) 29.970;
		cfps = (float) 24.000;
		current_num = 24;
		current_den = 1;
	}
	else if (Rate == CONVERT_25000_TO_29970)
	{
		tfps = (float) 29.970;
		cfps = (float) 25.000;
		current_num = 25;
		current_den = 1;
	}
	else if (Rate == CONVERT_CUSTOM)
	{
		if (strchr(InputRate, '/') != NULL)
		{
			// Have a fraction specified.
			char *p;

			p = InputRate;
			sscanf(p, "%I64Ld", &current_num);
			while (*p++ != '/');
			sscanf(p, "%I64Ld", &current_den);
		}
		else
		{
			// Have a decimal specified.
			float f;

			sscanf(InputRate, "%f", &f);
			current_num = (int64_t) (f * 1000000.0);
			current_den = 1000000;
		}

		if(0) { //debug
			sprintf(buf, "%I64Ld", current_num);
			printf("Current fps numerator: %s", buf);
			sprintf(buf, "%I64Ld", current_den);
			printf("Current fps denominator: %s", buf);
			return false;//FALSE;
		}

		sscanf(OutputRate, "%f", &tfps);
	}
	if (fabs(tfps - 23.976) < 0.01) // <-- we'll let people cheat a little here (ie 23.98)
	{
		rate = 1;
		rounded_fps = 24;
		drop_frame = 0x80;
		target_num = 24000; target_den = 1001;
	}
	else if (fabs(tfps - 24.000) < 0.001)
	{
		rate = 2;
		rounded_fps = 24;
		drop_frame = 0;
		target_num = 24; target_den = 1;
	}
	else if (fabs(tfps - 25.000) < 0.001)
	{
		rate = 3;
		rounded_fps = 25;
		drop_frame = 0;
		target_num = 25; target_den = 1;
	}
	else if (fabs(tfps - 29.970) < 0.001)
	{
		rate = 4;
		rounded_fps = 30;
		drop_frame = 0x80;
		target_num = 30000; target_den = 1001;
	}
	else if (fabs(tfps - 30.000) < 0.001)
	{
		rate = 5;
		rounded_fps = 30;
		drop_frame = 0;
		target_num = 30; target_den = 1;
	}
	else if (fabs(tfps - 50.000) < 0.001)
	{
		rate = 6;
		rounded_fps = 50;
		drop_frame = 0;
		target_num = 50; target_den = 1;
	}
	else if (fabs(tfps - 59.940) < 0.001)
	{
		rate = 7;
		rounded_fps = 60;
		drop_frame = 0x80;
		target_num = 60000; target_den = 1001;
	}
	else if (fabs(tfps - 60.000) < 0.001)
	{
		rate = 8;
		rounded_fps = 60;
		drop_frame = 0;
		target_num = 60; target_den = 1;
	}
	else
	{
		printf("Target rate is not a legal MPEG2 rate\n");
		return false;
	}

	// Make current fps = target fps for "No change"
	if (Rate == CONVERT_NO_CHANGE)
	{
		current_num = target_num;
		current_den = target_den;
		// no reason to reset rate
		rate = -1;
	}

	// equate denominators
	if (current_den != target_den) {
		if (current_den == 1)
			current_num *= (current_den = target_den);
		else if (target_den == 1)
			target_num *= (target_den = current_den);
		else {
			current_num *= target_den;
			target_num *= current_den;
			current_den = (target_den *= current_den);
		}
	}
	// make divisible by two
	if ((current_num & 1) || (target_num & 1)) {
		current_num <<= 1;
		target_num <<= 1;
		current_den = (target_den <<= 1);
	}

	if(0) { //debug
		sprintf(buf, "%I64Ld", current_num);
		printf("Current fps numerator: %s", buf);
		sprintf(buf, "%I64Ld", current_den);
		printf("Current fps denominator: %s", buf);
		sprintf(buf, "%I64Ld", target_num);
		printf("Target fps numerator: %s", buf);
		sprintf(buf, "%I64Ld", target_den);
		printf("Target fps denominator: %s", buf);
		return false;
	}

	if (((target_num - current_num) >> 1) > current_num)
	{
		printf("target rate/current rate\nmust not be greater than 1.5\n");
		return false;
	}
	else if (target_num < current_num)
	{
		printf("target rate/current rate\nmust not be less than 1.0\n");
		return false;
	}

	// set up df and tc vars
	if (TimeCodes)
	{
		// if the user wants to screw up the timecode... why not
		if (DropFrames == 0)
		{
			drop_frame = 0;
		}
		else if (DropFrames == 1)
		{
			drop_frame = 0x80;
		}
		// get timecode start (only if set tc is checked too, though)
		if (StartTime)
		{
			if (sscanf(HH, "%d", &hour) < 1) hour = 0;
			else if (hour < 0) hour = 0;	
			else if (hour > 23) hour = 23;
			if (sscanf(MM, "%d", &minute) < 1) minute = 0;
			else if (minute < 0) minute = 0;
			else if (minute > 59) minute = 59;
			if (sscanf(SS, "%d", &sec) < 1) sec = 0;
			else if (sec < 0) sec = 0;
			else if (sec > 59) sec = 59;
			if (sscanf(FF, "%d", &pict) < 1) pict = 0;
			else if (pict < 0) pict = 0;
			else if (pict >= rounded_fps) pict = rounded_fps - 1;
		}
		set_tc = 1;
	} else set_tc = 0;

	// Determine field_count for timecode start
	pict = (((hour*60)+minute)*60+sec)*rounded_fps+pict;
	if (drop_frame) pict -= 2 * (pict/1800 - pict/18000);
	field_count = pict << 1;

	// Recalc timecode and rewrite boxes
	if (drop_frame) pict += 18*(pict/17982) + 2*((pict%17982 - 2) / 1798);
	pict -= (sec = pict / rounded_fps) * rounded_fps; 
	sec -= (minute = sec / 60) * 60; 
	minute -= (hour = minute / 60) * 60;
	hour %= 24;

	sprintf(buf, "%02d",hour);
	strcpy(HH, buf);
	sprintf(buf, "%02d",minute);
	strcpy(MM, buf);
	sprintf(buf, "%02d",sec);
	strcpy(SS, buf);
	sprintf(buf, "%02d",pict);
	strcpy(FF, buf);
	return true;
}

void generate_flags(void)
{
	// Yay for integer math
	unsigned char *p, *q;
	unsigned int i,trfp;
	int64_t dfl,tfl;

	dfl = (target_num - current_num) << 1;
	tfl = current_num >> 1;

	// Generate BFF & TFF flags.
	p = bff_flags;
	q = tff_flags;
	trfp = 0;
	for (i = 0; i < MAX_PATTERN_LENGTH; i++)
	{
		tfl += dfl;
		if (tfl >= current_num)
		{ 
			tfl -= current_num; 
			*p++ = (trfp + 1);
			*q++ = ((trfp ^= 2) + 1);
		}
		else
		{
			*p++ = trfp;
			*q++ = (trfp ^ 2);
		}
	}
}
