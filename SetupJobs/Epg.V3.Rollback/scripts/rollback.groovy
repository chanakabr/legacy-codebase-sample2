ctx._source.remove('__documentTransactionalStatus');
ctx._routing = ctx._source['start_date'].substring(0,8);
ctx.remove('_parent');
ctx._id = ctx._source['epg_id'];
