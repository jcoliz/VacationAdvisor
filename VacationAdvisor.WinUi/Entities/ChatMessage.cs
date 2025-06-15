using System;
using System.Collections.Generic;
using System.Linq;

namespace VacationAdvisor.WinUi.Entities;

public record ChatMessage
{
    public enum RoleType
    {
        User,
        Assistant
    }

    public record class Content
    {
        public enum ContentType
        {
            Unsupported,
            Text,
            Image,
        }

        public ContentType Type { get; set; } = ContentType.Unsupported;

        public string? Text { get; set; } = null;

        public string? ImageId { get; set; } = null;
    }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public RoleType Role { get; set; } = RoleType.User;

    public ICollection<Content> Contents { get; set; } = new List<Content>();

    public string? Text => Contents
        .Where(x => x.Type == Content.ContentType.Text)
        .FirstOrDefault()?
        .Text;
}
