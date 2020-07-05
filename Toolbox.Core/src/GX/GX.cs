using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core.GX
{
    //Enums from https://github.com/magcius/noclip.website/blob/e5c302ff52ad72429e5d0dc64062420546010831/src/gx/gx_enum.ts
    public enum GXOpCodes : uint
    {
        NOOP = 0x00,
        DRAW_QUADS = 0x80,
        // Early code for GX_DRAW_QUADS? Seen in Luigi's Mansion.
        DRAW_QUADS_2 = 0x88,
        DRAW_TRIANGLES = 0x90,
        DRAW_TRIANGLE_STRIP = 0x98,
        DRAW_TRIANGLE_FAN = 0xA0,
        DRAW_LINES = 0xA8,
        DRAW_LINE_STRIP = 0xB0,
        DRAW_POINTS = 0xB8,

        LOAD_INDX_A = 0x20,
        LOAD_INDX_B = 0x28,
        LOAD_INDX_C = 0x30,
        LOAD_INDX_D = 0x38,

        LOAD_BP_REG = 0x61,
        LOAD_CP_REG = 0x08,
        LOAD_XF_REG = 0x10,
    }

    public enum GXAttributes : uint
    {
        PosNormMatrix = 1 << 0,
        Tex0Matrix = 1 << 1,
        Tex1Matrix = 1 << 2,
        Tex2Matrix = 1 << 3,
        Tex3Matrix = 1 << 4,
        Tex4Matrix = 1 << 5,
        Tex5Matrix = 1 << 6,
        Tex6Matrix = 1 << 7,
        Tex7Matrix = 1 << 8,
        Position = 1 << 9,
        Normal = 1 << 10,
        Color0 = 1 << 11,
        Color1 = 1 << 12,
        TexCoord0 = 1 << 13,
        TexCoord1 = 1 << 14,
        TexCoord2 = 1 << 15,
        TexCoord3 = 1 << 16,
        TexCoord4 = 1 << 17,
        TexCoord5 = 1 << 18,
        TexCoord6 = 1 << 19,
        TexCoord7 = 1 << 20,
        PositionMatrixArray = 1 << 21,
        NormalMatrixArray = 1 << 22,
        TexMatrixArray = 1 << 23,
        LightArray = 1 << 24,
        NormalBinormalTangent = 1 << 25,
    }

    public enum GXAttributeType : uint
    {
        NONE = 0,
        DIRECT = 1,
        INDEX8 = 2,
        INDEX16 = 3,
    }

    public enum GXTexFilter : uint
    {
        NEAR = 0, /*!< Point sampling, no mipmap */
        LINEAR = 1, /*!< Bilinear filtering, no mipmap */
        NEAR_MIP_NEAR = 2, /*!< Point sampling, discrete mipmap */
        LIN_MIP_NEAR = 3, /*!< Bilinear filtering, discrete mipmap */
        NEAR_MIP_LIN = 4, /*!< Point sampling, linear mipmap */
        LIN_MIP_LIN = 5, /*!< Trilinear filtering */
    }

    public enum GXComponentContent : uint
    {
        // Position
        POS_XY = 0,
        POS_XYZ = 1,
        // Normal
        NRM_XYZ = 0,
        NRM_NBT = 1,
        NRM_NBT3 = 2,
        // Color
        CLR_RGB = 0,
        CLR_RGBA = 1,
        // TexCoord
        TEX_S = 0,
        TEX_ST = 1,
    }

    public enum GXComponentType : uint
    {
        U8 = 0,
        S8 = 1,
        U16 = 2,
        S16 = 3,
        F32 = 4,

        RGB565 = 0,
        RGB8 = 1,
        RGBX8 = 2,
        RGBA4 = 3,
        RGBA6 = 4,
        RGBA8 = 5,
    }

    public enum GXCompareType : uint
    {
        NEVER = 0,
        LESS = 1,
        EQUAL = 2,
        LEQUAL = 3,
        GREATER = 4,
        NEQUAL = 5,
        GEQUAL = 6,
        ALWAYS = 7,
    }

    public enum GXAlphaOp : uint
    {
        AND = 0,
        OR = 1,
        XOR = 2,
        XNOR = 3,
    }

    public enum GXCullMode : uint
    {
        NONE = 0, /*!< Do not cull any primitives. */
        FRONT = 1, /*!< Cull front-facing primitives. */
        BACK = 2, /*!< Cull back-facing primitives. */
        ALL = 3, /*!< Cull all primitives. */
    }

    public enum GXBlendMode : uint
    {
        NONE = 0,
        BLEND = 1,
        LOGIC = 2,
        SUBTRACT = 3,
    }

    public enum GXBlendFactor : uint
    {
        ZERO = 0,
        ONE = 1,
        SRCCLR = 2,
        INVSRCCLR = 3,
        SRCALPHA = 4,
        INVSRCALPHA = 5,
        DSTALPHA = 6,
        INVDSTALPHA = 7,
    }

    public enum GXLogicOp : uint
    {
        CLEAR = 0,
        AND = 1,
        REVAND = 2,
        COPY = 3,
        INVAND = 4,
        NOOP = 5,
        XOR = 6,
        OR = 7,
        NOR = 8,
        EQUIV = 9,
        INV = 10,
        REVOR = 11,
        INVCOPY = 12,
        INVOR = 13,
        NAND = 14,
        SET = 15,
    }

    public enum GXTevOp : uint
    {
        ADD = 0,
        SUB = 1,
        COMP_R8_GT = 8,
        COMP_R8_EQ = 9,
        COMP_GR16_GT = 10,
        COMP_GR16_EQ = 11,
        COMP_BGR24_GT = 12,
        COMP_BGR24_EQ = 13,
        COMP_RGB8_GT = 14,
        COMP_RGB8_EQ = 15,
    }

    public enum GXTevBias : uint
    {
        ZERO = 0,
        ADDHALF = 1,
        SUBHALF = 2,

        // Used to denote the compare ops to the HW.
        HWB_COMPARE = 3,
    }

    public enum GXTevScale : uint
    {
        SCALE_1 = 0,
        SCALE_2 = 1,
        SCALE_4 = 2,
        DIVIDE_2 = 3,
    }
}
