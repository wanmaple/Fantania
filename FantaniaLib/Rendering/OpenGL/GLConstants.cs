using Avalonia.OpenGL;

namespace FantaniaLib;

public static class GLConstants
{
    public const int GL_NO_ERROR = GlConsts.GL_NO_ERROR;

    public const int GL_TRUE = 1;
    public const int GL_FALSE = 0;

    public const int GL_FLOAT = GlConsts.GL_FLOAT;

    public const int GL_UNSIGNED_BYTE = GlConsts.GL_UNSIGNED_BYTE;
    public const int GL_UNSIGNED_SHORT = GlConsts.GL_UNSIGNED_SHORT;
    public const int GL_RED = 0x1903;
    public const int GL_RGB = 0x1907;
    public const int GL_RGBA = GlConsts.GL_RGBA;
    public const int GL_R8 = 0x8229;
    public const int GL_RGB8 = 0x8051;
    public const int GL_RGBA8 = GlConsts.GL_RGBA8;
    public const int GL_SRGB8 = 0x8C41;
    public const int GL_SRGB8_ALPHA8 = 0x8C43;

    public const int GL_NEAREST = GlConsts.GL_NEAREST;
    public const int GL_LINEAR = GlConsts.GL_LINEAR;
    public const int GL_NEAREST_MIPMAP_NEAREST = 0x2700;
    public const int GL_LINEAR_MIPMAP_NEAREST = 0x2701;
    public const int GL_NEAREST_MIPMAP_LINEAR = 0x2702;
    public const int GL_LINEAR_MIPMAP_LINEAR = 0x2703;
    public const int GL_CLAMP_TO_EDGE = 0x812F;
    public const int GL_CLAMP_TO_BORDER = 0x812D;
    public const int GL_REPEAT = 0x2901;
    public const int GL_MIRRORED_REPEAT = 0x8370;
    public const int GL_MIRROR_CLAMP_TO_EDGE = 0x8743;
    public const int GL_FRAMEBUFFER = GlConsts.GL_FRAMEBUFFER;
    public const int GL_FRAMEBUFFER_BINDING = GlConsts.GL_FRAMEBUFFER_BINDING;
    public const int GL_FRAMEBUFFER_COMPLETE = GlConsts.GL_FRAMEBUFFER_COMPLETE;
    public const int GL_COLOR_ATTACHMENT0 = GlConsts.GL_COLOR_ATTACHMENT0;
    public const int GL_DEPTH_ATTACHMENT = GlConsts.GL_DEPTH_ATTACHMENT;
    public const int GL_RENDERBUFFER = GlConsts.GL_RENDERBUFFER;
    public const int GL_DEPTH24_STENCIL8 = GlConsts.GL_DEPTH24_STENCIL8;

    public const int GL_COLOR_BUFFER_BIT = GlConsts.GL_COLOR_BUFFER_BIT;
    public const int GL_DEPTH_BUFFER_BIT = GlConsts.GL_DEPTH_BUFFER_BIT;
    public const int GL_STENCIL_BUFFER_BIT = GlConsts.GL_STENCIL_BUFFER_BIT;

    public const int GL_DEPTH_TEST = GlConsts.GL_DEPTH_TEST;
    public const int GL_BLEND = 0x0BE2;
    public const int GL_SRC_ALPHA = 0x0302;
    public const int GL_ONE_MINUS_SRC_ALPHA = 0x0303;
    public const int GL_SRC_COLOR = 0x0300;
    public const int GL_ONE_MINUS_SRC_COLOR = 0x0301;
    public const int GL_DST_ALPHA = 0x0304;
    public const int GL_ONE_MINUS_DST_ALPHA = 0x0305;
    public const int GL_DST_COLOR = 0x0306;
    public const int GL_ONE_MINUS_DST_COLOR = 0x0307;
    public const int GL_ZERO = 0;
    public const int GL_ONE = 1;

    public const int GL_ARRAY_BUFFER = GlConsts.GL_ARRAY_BUFFER;
    public const int GL_ELEMENT_ARRAY_BUFFER = GlConsts.GL_ELEMENT_ARRAY_BUFFER;
    public const int GL_STATIC_DRAW = GlConsts.GL_STATIC_DRAW;
    public const int GL_TRIANGLES = GlConsts.GL_TRIANGLES;

    public const int GL_TEXTURE_MIN_FILTER = GlConsts.GL_TEXTURE_MIN_FILTER;
    public const int GL_TEXTURE_MAG_FILTER = GlConsts.GL_TEXTURE_MAG_FILTER;
    public const int GL_TEXTURE_WRAP_S = 0x2802;
    public const int GL_TEXTURE_WRAP_T = 0x2803;
    public const int GL_TEXTURE_1D = 0x0DE0;
    public const int GL_TEXTURE_2D = GlConsts.GL_TEXTURE_2D;
    public const int GL_TEXTURE0 = GlConsts.GL_TEXTURE0;

    public const int GL_FRONT = 0x0404;
    public const int GL_BACK = 0x0405;
    public const int GL_FRONT_AND_BACK = 0x0408;

    public const int GL_VERTEX_SHADER = GlConsts.GL_VERTEX_SHADER;
    public const int GL_FRAGMENT_SHADER = GlConsts.GL_FRAGMENT_SHADER;
    public const int GL_FRAMEBUFFER_SRGB = 0x8DB9;
    public const int GL_UNPACK_ALIGNMENT = 0x0CF5;
    public const int GL_PACK_ALIGNMENT = 0x0D05;
}