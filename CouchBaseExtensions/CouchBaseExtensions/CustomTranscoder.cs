using Couchbase.Core.Serialization;
using Couchbase.Core.Transcoders;
using Couchbase.IO.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CouchBaseExtensions
{
    public class CustomTranscoder : ITypeTranscoder
    {
        Couchbase.Core.Transcoders.DefaultTranscoder DefualtTranscoder;

        public CustomTranscoder()
        {
            DefualtTranscoder = new DefaultTranscoder();
        }

        public CustomTranscoder(ITypeSerializer serializer)
        {
            DefualtTranscoder = new DefaultTranscoder() { Serializer = serializer };
        }

        public CustomTranscoder(IByteConverter converter)
        {
            DefualtTranscoder = new DefaultTranscoder(converter);
        }

        public CustomTranscoder(IByteConverter converter, ITypeSerializer serializer)
        {
            DefualtTranscoder = new DefaultTranscoder(converter, serializer);
        }

        public IByteConverter Converter
        {
            get
            {
                return DefualtTranscoder.Converter;
            }
            set
            {
                DefualtTranscoder.Converter = value;
            }
        }

        public T Decode<T>(byte[] buffer, int offset, int length, Couchbase.IO.Operations.Flags flags, Couchbase.IO.Operations.OperationCode opcode)
        {
            return DefualtTranscoder.Decode<T>(buffer, offset, length, flags, opcode);
        }

        public T Decode<T>(ArraySegment<byte> buffer, int offset, int length, Couchbase.IO.Operations.Flags flags, Couchbase.IO.Operations.OperationCode opcode)
        {
            return DefualtTranscoder.Decode<T>(buffer, offset, length, flags, opcode);
        }

        public byte[] Encode<T>(T value, Couchbase.IO.Operations.Flags flags, Couchbase.IO.Operations.OperationCode opcode)
        {
            return DefualtTranscoder.Encode<T>(value, flags, opcode);
        }

        public Couchbase.IO.Operations.Flags GetFormat<T>(T value)
        {
            Couchbase.IO.Operations.Flags flags = DefualtTranscoder.GetFormat<T>(value);            
            if (flags.TypeCode == TypeCode.Object) 
            { 
                flags.TypeCode = TypeCode.String; 
            }
            return flags;
        }

        public Couchbase.Core.Serialization.ITypeSerializer Serializer
        {
            get
            {
                return DefualtTranscoder.Serializer;
            }
            set
            {
                DefualtTranscoder.Serializer = value;
            }
        }
    }
}