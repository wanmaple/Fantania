FantaniaTextures = {
    White4x4 = {
        type = 0,
        params = "path:avares://Fantania/Assets/textures/white4x4.png",
    },
}

EditControls = {
    CheckBox = "FantaniaLib.CheckBox",
    IntegerBox = "FantaniaLib.IntegerBox",
    FloatBox = "FantaniaLib.FloatBox",
    StringBox = "FantaniaLib.StringBox",
    Vector2Box = "FantaniaLib.Vector2Box",
    Vector2IntBox = "FantaniaLib.Vector2IntBox",
    ColorPicker = "FantaniaLib.ColorPicker",
    TextureBox = "FantaniaLib.TextureBox",
}

BuiltinStages = {
    TiledLightCulling = "Tiled Light Culling",
    LightOccluderSDF = "Light Occluder SDF",
    Opaque = "Opaque",
    Transparent = "Transparent",
    PostProcessing = "Post Processing",
}

BuiltinShaders = {
    VS_Standard = "avares://Fantania/Assets/shaders/vert_standard.vs",
    VS_StandardNoFlip = "avares://Fantania/Assets/shaders/vert_standard_noflip.vs",
    FS_Standard = "avares://Fantania/Assets/shaders/frag_standard.fs",
    FS_StandardCutoff = "avares://Fantania/Assets/shaders/frag_standard_cutoff.fs",
    FS_LightOccluderMask = "avares://Fantania/Assets/shaders/frag_light_occluder_mask.fs",
}

BuiltinTextureFilters = {
    PixelClamp = {
        minFilter = TextureMinFilters.Nearest,
        magFilter = TextureMagFilters.Nearest,
        wrapS = TextureWraps.ClampToEdge,
        wrapT = TextureWraps.ClampToEdge,
    },
    PixelRepeat = {
        minFilter = TextureMinFilters.Nearest,
        magFilter = TextureMagFilters.Nearest,
        wrapS = TextureWraps.Repeat,
        wrapT = TextureWraps.Repeat,
    },
    LinearClamp = {
        minFilter = TextureMinFilters.Linear,
        magFilter = TextureMagFilters.Linear,
        wrapS = TextureWraps.ClampToEdge,
        wrapT = TextureWraps.ClampToEdge,
    },
    LinearRepeat = {
        minFilter = TextureMinFilters.Linear,
        magFilter = TextureMagFilters.Linear,
        wrapS = TextureWraps.Repeat,
        wrapT = TextureWraps.Repeat,
    },
}