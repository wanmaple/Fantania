local EditorSetup = {
	gridAlign = 16,
	zoomSensitivity = 1.5,
	zoomMin = 0.25,
	zoomMax = 4.0,
	-- Render Layer names, should be in range [-40, 39]
	namedLayers = {
		[0] = "Default",
		[39] = "Background",
		[-40] = "Foreground",
		[-10] = "Platforms",
	},
}

return EditorSetup