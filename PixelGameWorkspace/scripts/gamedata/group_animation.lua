local GroupAnimation = Class("GroupAnimation");

GroupAnimation.name = "SN_GroupAnimation"
GroupAnimation.tooltip = "ST_GroupAnimation"
GroupAnimation.dataGroup = "Animation"
GroupAnimation.dataDefs = {
    animations = {
        type = FieldTypes.GroupReferenceArray,
    },
}
GroupAnimation.editDefs = {
    animations = {
        group = "SG_Data",
        tooltip = "ST_GroupAnimation_Animations",
        defaultMemberValue = {
            type = "Animation",
            id = 0,
        },
    },
}

return GroupAnimation