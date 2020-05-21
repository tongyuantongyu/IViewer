// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.h"
#include "EasyBMP.h"

#define export __declspec(dllexport)

typedef struct BMPExport {
	uint32_t width;
	uint32_t height;
	BMP* bmp; // obscure for extern use.
} BMPExport;

#ifdef __cplusplus
extern "C" {
#endif

export BMPExport* DecodeFromBuffer(const char* buffer, const size_t length) {
	auto bmp = new BMPExport{0, 0, nullptr};
	bmp->bmp = new BMP;
	const auto result = bmp->bmp->ReadFromBuffer(buffer, length);
	if (!result) {
		delete bmp->bmp;
		bmp->bmp = nullptr;
		return bmp;
	}
	bmp->width = bmp->bmp->TellWidth();
	bmp->height = bmp->bmp->TellHeight();
	return bmp;
}

export bool WriteToMemory(BMPExport* bmp, uint8_t* destination, const size_t stride) {
	if (bmp->bmp == nullptr || stride < (size_t)bmp->width * 4) {
		return false;
	}
	
	for (size_t i = 0; i < bmp->height; ++i) {
		const auto begin = destination + stride * i;
		if (!bmp->bmp->Write32bitRow(begin, stride, i)) {
			return false;
		}
	}
	return true;
}

export void FreeBMP(BMPExport* bmp) {
	delete bmp->bmp;
} 

#ifdef __cplusplus
}
#endif

