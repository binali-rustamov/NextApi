using System.Collections.ObjectModel;
using Abitech.NextApi.Common.DTO;
using Abitech.NextApi.Server.Tests.EntityService.Model;

namespace Abitech.NextApi.Server.Tests.EntityService.DTO
{
    public class TestTreeItemDto: IEntityDto<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public virtual TestTreeItemDto Parent { get; set; }
        
        public virtual Collection<TestTreeItemDto> Children { get; set; }
    }
}
