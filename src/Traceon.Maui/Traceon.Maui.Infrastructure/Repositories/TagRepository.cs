using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Infrastructure.Data;

namespace Arisoul.Traceon.Maui.Infrastructure.Repositories;

public class TagRepository
    : BaseRepository<Tag>, ITagRepository
{
    public TagRepository(TraceonDbContext context) 
        : base(context)
    {
    }
}