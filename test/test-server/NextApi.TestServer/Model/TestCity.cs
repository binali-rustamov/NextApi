using System;
using System.ComponentModel.DataAnnotations;
using Abitech.NextApi.UploadQueue.Common.Entity;

namespace NextApi.TestServer.Model
{
    public class TestCity : IUploadQueueEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public int? SomeNullableInt { get; set; }
        public int Population { get; set; }
        public string Demonym { get; set; }
        public Guid GuidProp { get; set; }
        public Guid? NullableGuidProp { get; set; }
    }
}
