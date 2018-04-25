// © 2018 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using Sitecore.Analytics.Data;
using Sitecore.Analytics.Pipelines.InitializeTracker;
using Sitecore.Analytics.Tracking;
using Sitecore.ExM.Framework.Diagnostics;
using Sitecore.Framework.Conditions;
using Sitecore.Modules.EmailCampaign.Core;

namespace Sitecore.Support.Modules.EmailCampaign.Core.Pipelines.EnsureSessionContext
{
  /// <summary>
  /// A processor for the ensureSessionContext pipeline. The processor detects if the current session is an
  /// email rendering session and if the recipient is an xDB contact it will load the contact into the
  /// session for personalization.
  /// </summary>
  [UsedImplicitly]
  public class LoadEmailRenderSessionContact
  {
    private readonly ILogger _logger;

    /// <summary>
    /// Gets or sets the contact repository.
    /// Note: We are not using the <see cref="ContactManager"/> as that will not load a contact on XP1 installations
    /// </summary>
    public ContactRepositoryBase ContactRepository { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadEmailRenderSessionContact"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/></param>
    public LoadEmailRenderSessionContact([NotNull] ILogger logger)
    {
      Condition.Requires(logger, nameof(logger)).IsNotNull();

      _logger = logger;
    }

    /// <summary>
    /// Loads the recipient contact from xDB and assigns it to the session.
    /// If the session is not for email rendering this processor has no effect.
    /// </summary>
    /// <param name="args">The pipeline argument.</param>
    public void Process(InitializeTrackerArgs args)
    {
      if (!ExmContext.IsRenderRequest || ExmContext.ContactIdentifier == null || ContactRepository == null)
      {
        return;
      }

      Contact contact = GetContact();

      if (contact == null)
      {
        return;
      }

      args.Session.Contact = contact;
    }

    protected virtual Contact GetContact()
    {
      try
      {
        return ContactRepository.LoadContact(ExmContext.ContactIdentifier.Source, ExmContext.ContactIdentifier.Identifier);
      }
      catch (Exception e)
      {
        _logger.LogError(e);
      }

      return null;
    }
  }
}
