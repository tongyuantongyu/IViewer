#include "pch.h"

#include <cstdint>
#include <algorithm>
#include <cmath>

#include "table.h"

#ifdef __cplusplus
extern "C" {
#endif

// flip color order. (for 8bpp image)
__declspec(dllexport) void BGRA2RGBA(uint32_t* begin, const size_t length) {
	auto scanner = begin;
	const auto end = scanner + length;
	while (scanner != end) {
		*scanner = (*scanner & 0xFF00FF00u) | (*scanner & 0x00FF0000u) >> 16 | (*scanner &
			0x000000FFu) << 16;
		++scanner;
	}
}

// flip color order. (for 16bpp image, without alpha)
__declspec(dllexport) void BBGGRR2RRGGBB(uint16_t* begin, const size_t length) {
	auto scanner = begin;
	const auto end = scanner + length * 3;
	while (scanner < end) {
		const auto _tmp = *scanner;
		*scanner = *(scanner + 2);
		*(scanner + 2) = _tmp;
		scanner += 3;
	}
}

// flip color order. (for 16bpp image, with alpha)
__declspec(dllexport) void BBGGRRAA2RRGGBBAA(uint16_t* begin, const size_t length) {
	auto scanner = begin;
	const auto end = scanner + length * 3;
	while (scanner < end) {
		const auto _tmp = *scanner;
		*scanner = *(scanner + 2);
		*(scanner + 2) = _tmp;
		scanner += 4;
	}
}

// convert 10/12 bit rgba to 16bit
__declspec(dllexport) void DEPTHCONVERT(uint16_t* begin, const size_t length, const size_t bit_depth) {
	auto scanner = begin;
	const auto end = scanner + length;
	if (bit_depth > 8 && bit_depth < 16) {
		const auto shift = 16 - bit_depth;
		while (scanner != end) {
			*scanner = _rotl16(*scanner, shift);
			++scanner;
		}
	}
}

// fix strange behavior on GDI+ based renderer
__declspec(dllexport) void GDICOLORFIX(uint16_t* begin, const size_t length, const double gamma) {
	auto scanner = begin;
	const auto end = scanner + length;
	if (gamma - 2.2 < 0.05) {
		while (scanner != end) {
			*scanner = GAMMA_2_2_TABLE[*scanner >> 3];
			++scanner;
		}
	}
	else {
		const long double regulator = std::pow(8191, -gamma + 1);
		while (scanner != end) {
			*scanner = std::round(exp2l(gamma * log2l(*scanner >> 3)) * regulator);
			++scanner;
		}
	}
}


#ifdef __cplusplus
}
#endif
