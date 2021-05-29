/*API.onEntityStreamIn.connect(function (ent, entType) {
    if (entType == 6 || entType == 8) {// Player or ped
        if(!API.hasEntitySyncedData(ent, "id_text_label"))
        {
            if (API.hasEntitySyncedData(ent, "nethandle_id")) {
                var handle = API.getEntitySyncedData(ent, "nethandle_id");
                let label = API.createTextLabel("(" + handle + ")", API.getEntityPosition(ent), API.toFloat(40), API.toFloat(0.3));
                API.attachEntity(label, ent, "", new Vector3(0, 0, 2), new Vector3(0, 0, 0));
                API.setTextLabelColor(label, 180, 10, 10, 255);
                API.setEntitySyncedData(ent, "id_text_label", label);
            }
        }
    }
});

API.onEntityStreamOut.connect(function (ent, entType) {
    if (entType == 6 || entType == 8) {// Player or ped
        if(API.hasEntitySyncedData(ent, "id_text_label"))
        {
            let label = API.getEntitySyncedData(ent, "id_text_label");
            API.deleteTextLabel(label);
            API.resetEntitySyncedData(ent, "id_text_label");
        }
    }
});
*/