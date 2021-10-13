using System.Linq;
using CommonServiceLocator;
using DigitalRune.Graphics;
using Kesmai.WorldForge.Editor;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Kesmai.WorldForge.MVP
{
	public class VisibilityOptions : ObservableRecipient
	{
		private bool _openDoors;
		private bool _hideSecretDoors;
		private bool _showTeleporters;
		private bool _showSpawns;
		
		/// <summary>
		/// Gets or sets a value indicating whether to doors are open.
		/// </summary>
		public bool OpenDoors
		{
			get => _openDoors;
			set
			{
				SetProperty(ref _openDoors, value);
				WeakReferenceMessenger.Default.Send<VisibilityOptionsChanged>();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to doors shown in their 'secret' state.
		/// </summary>
		public bool HideSecretDoors
		{
			get => _hideSecretDoors;
			set
			{
				SetProperty(ref _hideSecretDoors, value);
				WeakReferenceMessenger.Default.Send<VisibilityOptionsChanged>();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether Teleporter source and destinations are highlighted
		/// </summary>
		public bool ShowTeleporters
		{
			get => _showTeleporters;
			set
			{
				SetProperty(ref _showTeleporters, value);
				WeakReferenceMessenger.Default.Send<VisibilityOptionsChanged>();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether Spawn regions are highlighted
		/// </summary>
		public bool ShowSpawns
		{
			get => _showSpawns;
			set
			{
				SetProperty(ref _showSpawns, value);
				WeakReferenceMessenger.Default.Send<VisibilityOptionsChanged>();
			}
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="VisibilityOptions"/> class.
		/// </summary>
		public VisibilityOptions()
		{
			OpenDoors = false;
			HideSecretDoors = false;
			ShowTeleporters = false;
			ShowSpawns = false;
		}
	}
	public class VisibilityOptionsChanged
	{
		public VisibilityOptionsChanged()
		{
		}
	}
}