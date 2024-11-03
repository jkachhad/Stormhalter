using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Engines.Gumps;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Engines.Library;

namespace Kesmai.Server.Items;

public class Book : ItemEntity
{
	private string _publicationId;
	
	/// <inheritdoc />
	public override int Weight => 5;

	/// <inheritdoc />
	public override int LabelNumber => 6000011;

	/// <inheritdoc />
	public override int Category => ItemCategory.Miscellaneous.Id;

	/// <summary>
	/// Gets or sets the publication id.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public string PublicationId
	{
		get => _publicationId;
		set
		{
			var oldValue = _publicationId;
			var newValue = value;

			if (oldValue != newValue)
			{
				_publicationId = newValue;
				
				if (Library.TryGetPublication(_publicationId, out var libraryBook))
					Publication = libraryBook;
			}
		}
	}

	/// <summary>
	/// Gets the publication.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public Publication Publication { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Book"/> class.
	/// </summary>
	[WorldForge]
	public Book(string publicationId, int bookId = 152) : base(bookId)
	{
		PublicationId = publicationId;
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Book"/> class.
	/// </summary>
	public Book(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		if (Publication is not null)
			entries.Add(new LocalizationEntry(
				6200259, Publication.Title, Publication.Author)); /* [You are looking at a book entitled '{0}' written by {1}.] */
	}

	/// <inheritdoc />
	public override ActionType GetAction()
	{
		var parent = Parent;

		if (parent is MobileEntity mobile)
			return ActionType.Look;

		return base.GetAction();
	}
	
	/// <inheritdoc />
	public override bool HandleInteraction(MobileEntity entity, ActionType action)
	{
		if (action != ActionType.Look)
			return base.HandleInteraction(entity, action);

		if (Publication is null)
			return false;
		


		if (entity is PlayerEntity player)
		{
			if (Publication is PublishedBook book)
			{
#if (DEBUG)
				book.Refresh();
#endif
				entity.CloseGumps<BookGump>();
				entity.SendGump(new BookGump(player, book));
			}
			else if (Publication is PublishedScroll scroll)
			{
#if (DEBUG)
				scroll.Refresh();
#endif
				entity.CloseGumps<ScrollGump>();
				entity.SendGump(new ScrollGump(player, scroll));
			}
		}

		return false;
	}

	/// <summary>
	/// Serializes this instance into binary data for persistence.
	/// </summary>
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1);	/* version */
		writer.Write(PublicationId);
	}

	/// <summary>
	/// Deserializes this instance from persisted binary data.
	/// </summary>
	public override void Deserialize(ref SpanReader reader)
	{
		base.Deserialize(ref reader);
			
		var version = reader.ReadInt16();

		switch (version)
		{
			case 1:
			{
				PublicationId = reader.ReadString();
				break;
			}
		}
	}
}

public class BookGump : Gump
{
	private readonly PlayerEntity _player;
	private readonly Publication _publication;

	private int _currentPage;

	/// <summary>
	/// Initializes a new instance of the <see cref="BookGump"/> class.
	/// </summary>
	public BookGump(PlayerEntity player, Publication publication)
	{
		_player = player;
		_publication = publication;

		Style = "Client-Book-Frame-Default";
		
		Overlay = true;
		CanDrag = true;
		
		RenderScale = 1.0f;
		
		var stackPanel = new StackPanel()
		{
			Style = "Client-Pages-Frame-Default",
			
			Orientation = Orientation.Horizontal,
			
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
		};
		
		var leftPanel = new StackPanel()
		{
			Name = "leftPage",
			Style = "Client-Page-Left-Default",
		};
		var rightPanel = new StackPanel()
		{
			Name = "rightPage",
			Style = "Client-Page-Right-Default",
		};
		
		stackPanel.Children.Add(leftPanel);
		stackPanel.Children.Add(rightPanel);
		
		Children.Add(stackPanel);
		
		/* get the right page. */
		var leftPage = _publication.GetPage(_currentPage);
		var rightPage = _publication.GetPage(_currentPage + 1);
		
		if (leftPage is not null)
			leftPanel.Children.Add(leftPage);
		
		if (rightPage is not null)
			rightPanel.Children.Add(rightPage);

	}
}

public class ScrollGump : Gump
{
	private readonly PlayerEntity _player;
	private readonly Publication _publication;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="ScrollGump"/> class.
	/// </summary>
	public ScrollGump(PlayerEntity player, Publication publication)
	{
		_player = player;
		_publication = publication;

		Style = "Client-Scroll-Frame-Default";
		
		Overlay = true;
		CanDrag = true;
		
		RenderScale = 1.0f;

		var stackPanel = new StackPanel()
		{
			Height = 1122 - 60,
		};
		
		Children.Add(stackPanel);
		
		// header panel
		var headerPanel = new StackPanel()
		{
			Orientation = Orientation.Horizontal,
			HorizontalAlignment = HorizontalAlignment.Stretch,
		};

		var titlePanel = new StackPanel()
		{
			Style = "Client-Scroll-Frame-Header",
			HorizontalAlignment = HorizontalAlignment.Stretch,
		};

		var titleText = new TextBlock()
		{
			Text = _publication.Title,

			FontSize = 24,
			Foreground = Color.Black,

			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Bottom,
		};

		var closeButton = new TextureButton()
		{
			Style = "Client-Scroll-Frame-Close",
			
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Top,
		};

		titlePanel.Children.Add(titleText);

		headerPanel.Children.Add(titlePanel);
		headerPanel.Children.Add(closeButton);
		
		// content
		var scrollViewer = new ScrollViewer()
		{
			Style = "Client-Scroll-ScrollViewer",
			
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			
			HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
			VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
			
			Margin = new Rectangle(0, 30, 0, 40)
		};

		var contentPanel = new StackPanel()
		{
			Width = 776 - 60 - 20,
		};
		
		// page
		var page = _publication.GetPage(0);

		if (page is not null)
		{
			foreach (var text in page.Children)
			{
				text.Stroke = default(Color);
				text.Foreground = Color.Black;
				text.FontStyle = MSDFStyle.Regular;
			}

			contentPanel.Children.Add(page);
		}
		
		scrollViewer.Content = contentPanel;
		
		stackPanel.Children.Add(headerPanel);
		stackPanel.Children.Add(scrollViewer);
	}
}