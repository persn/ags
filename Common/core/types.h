//=============================================================================
//
// Adventure Game Studio (AGS)
//
// Copyright (C) 1999-2011 Chris Jones and 2011-20xx others
// The full list of copyright holders can be found in the Copyright.txt
// file, which is part of this source code distribution.
//
// The AGS source code is provided under the Artistic License 2.0.
// A copy of this license can be found in the file License.txt and at
// http://www.opensource.org/licenses/artistic-license-2.0.php
//
//=============================================================================
//
// Basic types definition
//
//=============================================================================
#ifndef __AGS_CN_CORE__TYPES_H
#define __AGS_CN_CORE__TYPES_H

#include <stddef.h>
#include <stdint.h>
#include <stdlib.h> // for size_t
#include <limits.h> // for _WORDSIZE

#ifndef NULL
#define NULL nullptr
#endif

#ifndef FORCEINLINE
    #if defined(_MSC_VER)
        #define FORCEINLINE __forceinline
    #elif __has_attribute(always_inline)
        #define FORCEINLINE __attribute__((always_inline))
    #else
        #define FORCEINLINE
    #endif
#endif

// Stream offset type
typedef int64_t soff_t;

#define fixed_t int32_t // fixed point type
#define color_t int32_t

// TODO: use distinct fixed point class
enum
{
    kShift    = 16,
    kUnit     = 1 << kShift
};

#endif // __AGS_CN_CORE__TYPES_H
