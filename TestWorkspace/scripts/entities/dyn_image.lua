local StaticImage = require "entities.static_image"

local DynImage = Class("DynImage", StaticImage)

DynImage.name = "SN_DynImage"
DynImage.tooltip = "ST_DynImage"
DynImage.dataDefs["ext"] = {
	type = FieldTypes.Integer,
	default = 100,
}
DynImage.editDefs["ext"] = {
	group = "SG_Appearance",
	tooltip = "ST_DynImage_Ext",
}

return DynImage