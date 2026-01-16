local StaticImage = require "entities.static_image"

local DynImage = Class("DynImage", StaticImage)

function DynImage:name()
	return "SN_DynImage"
end

function DynImage:tooltip()
	return "ST_DynImage"
end

function DynImage:dataDefs()
	local defs = StaticImage.dataDefs(self)
	defs["ext"] = {
		type = FieldTypes.Integer,
		default = 100,
	}
	return defs
end

function DynImage:editDefs()
	local defs = StaticImage.editDefs(self)
	defs["ext"] = {
		group = "SG_Appearance",
		tooltip = "ST_DynImage_Ext",
	}
	return defs
end

return DynImage