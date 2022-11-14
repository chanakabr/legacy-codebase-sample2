ctx._source['__documentTransactionalStatus']='INSERTING';
ctx._routing = ctx._source['epg_channel_id'];
ctx._parent = "0_" + ctx._source['epg_channel_id'];
ctx._id = ctx._source['document_id'];
