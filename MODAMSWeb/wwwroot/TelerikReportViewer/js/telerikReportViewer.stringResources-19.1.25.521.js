(function () {
    'use strict';

    var sr = {
      // warning and error string resources
      controllerNotInitialized: "Controller is not initialized.",
      noReportInstance: "No report instance.",
      missingTemplate: "!obsolete resource!",
      noReport: "No report.",
      noReportDocument: "No report document.",
      missingOrInvalidParameter: "There are missing or invalid parameter values. Please input valid data for the following parameters:\n",
      invalidParameter: "Please input a valid value.",
      invalidDateTimeValue: "Please input a valid date.",
      parameterIsEmpty: "Parameter value cannot be empty.",
      cannotValidateType: "Cannot validate parameter of type {type}.",
      loadingFormats: "Loading...",
      loadingReport: "Loading report...",
      loadingParameters: "Loading parameters...",
      autoRunDisabled: "Please validate the report parameter values and press Preview to generate the report.",
      preparingDownload: "Preparing document to download. Please wait...",
      preparingPrint: "Preparing document to print. Please wait...",
      errorLoadingTemplates: "Error loading the report viewer's templates. (templateUrl = '{0}').",
      errorServiceUrl: "Cannot access the Reporting REST service. (serviceUrl = '{0}'). Make sure the service address is correct and enable CORS if needed. (https://enable-cors.org)",
      errorServiceVersion: "The version of the Report Viewer '{1}' does not match the version of the Reporting REST Service '{0}'. Please make sure both are running same version.",
      loadingReportPagesInProgress: "{0} pages loaded so far...",
      loadedReportPagesComplete: "Done. Total {0} pages loaded.",
      noPageToDisplay: "No page to display.",
      errorDeletingReportInstance: "Error deleting report instance: '{0}'.",
      errorRegisteringViewer: "Error registering the viewer with the service.",
      noServiceClient: "No serviceClient has been specified for this controller.",
      errorRegisteringClientInstance: "Error registering client instance.",
      errorCreatingReportInstance: "Error creating report instance (Report = '{0}').",
      errorCreatingReportDocument: "Error creating report document (Report = '{0}'; Format = '{1}').",
      unableToGetReportParameters: "Unable to get report parameters.",
      errorObtainingAuthenticationToken: "Error obtaining authentication token.",
      clientExpired: "Click 'Refresh' to restore client session.",
      promisesChainStopError: "Error shown. Throwing promises chain stop error.",
      renderingCancelled: "Report processing was canceled.",
      tryReportPreview: "The report may now be previewed.",
      // viewer template string resources
      parameterEditorSelectNone: "clear selection",
      parameterEditorSelectAll: "select all",
      parametersAreaPreviewButton: "Preview",
      menuNavigateBackwardText: "Navigate Backward",
      menuNavigateBackwardTitle: "Navigate Backward",
      menuNavigateForwardText: "Navigate Forward",
      menuNavigateForwardTitle: "Navigate Forward",
      menuStopRenderingText: "Stop Rendering",
      menuStopRenderingTitle: "Stop Rendering",
      menuRefreshText: "Refresh",
      menuRefreshTitle: "Refresh",
      menuFirstPageText: "First Page",
      menuFirstPageTitle: "First Page",
      menuLastPageText: "Last Page",
      menuLastPageTitle: "Last Page",
      menuPreviousPageTitle: "Previous Page",
      menuNextPageTitle: "Next Page",
      menuPageNumberTitle: "Page Number Selector",
      menuDocumentMapTitle: "Toggle Document Map",
      menuParametersAreaTitle: "Toggle Parameters Area",
      menuZoomInTitle: "Zoom In",
      menuZoomOutTitle: "Zoom Out",
      menuPageStateTitle: "Toggle FullPage/PageWidth",
      menuPrintText: "Print...",
      menuContinuousScrollText: "Toggle Continuous Scrolling",
      menuSendMailText: "Send an email",
      menuPrintTitle: "Print",
      menuContinuousScrollTitle: "Toggle Continuous Scrolling",
      menuSendMailTitle: "Send an email",
      menuExportText: "Export",
      menuExportTitle: "Export",
      menuPrintPreviewText: "Toggle Print Preview",
      menuPrintPreviewTitle: "Toggle Print Preview",
      menuSearchText: "Search",
      menuSearchTitle: "Toggle Search",
      menuAiPromptTitle: "Toggle AI Prompt",
      menuSideMenuTitle: "Toggle Side Menu",
      sendEmailFromLabel: "From:",
      sendEmailToLabel: "To:",
      sendEmailCCLabel: "CC:",
      sendEmailSubjectLabel: "Subject:",
      sendEmailFormatLabel: "Format:",
      sendEmailSendLabel: "Send",
      sendEmailCancelLabel: "Cancel",
      // accessibility string resources
      ariaLabelPageNumberSelector: "Page number selector. Showing page {0} of {1}.",
      ariaLabelPageNumberEditor: "Page number editor",
      ariaLabelExpandable: "Expandable",
      ariaLabelSelected: "Selected",
      ariaLabelParameter: "parameter",
      ariaLabelErrorMessage: "Error message",
      ariaLabelParameterInfo: "Contains {0} options",
      ariaLabelMultiSelect: "Multiselect",
      ariaLabelMultiValue: "Multivalue",
      ariaLabelSingleValue: "Single value",
      ariaLabelParameterDateTime: "DateTime",
      ariaLabelParameterString: "String",
      ariaLabelParameterNumerical: "Numerical",
      ariaLabelParameterBoolean: "Boolean",
      ariaLabelParametersAreaPreviewButton: "Preview the report",
      ariaLabelMainMenu: "Main menu",
      ariaLabelCompactMenu: "Compact menu",
      ariaLabelSideMenu: "Side menu",
      ariaLabelDocumentMap: "Document map area",
      ariaLabelDocumentMapSplitter: "Document map area splitbar.",
      ariaLabelParametersAreaSplitter: "Parameters area splitbar.",
      ariaLabelPagesArea: "Report contents area",
      ariaLabelSearchDialogArea: "Search area",
      ariaLabelAiPromptDialogArea: "AI prompt area",
      ariaLabelSendEmailDialogArea: "Send email area",
      ariaLabelSearchDialogStop: "Stop search",
      ariaLabelSearchDialogOptions: "Search options",
      ariaLabelSearchDialogNavigateUp: "Navigate up",
      ariaLabelSearchDialogNavigateDown: "Navigate down",
      ariaLabelSearchDialogMatchCase: "Match case",
      ariaLabelSearchDialogMatchWholeWord: "Match whole word",
      ariaLabelSearchDialogUseRegex: "Use regex",
      ariaLabelMenuNavigateBackward: "Navigate backward",
      ariaLabelMenuNavigateForward: "Navigate forward",
      ariaLabelMenuStopRendering: "Stop rendering",
      ariaLabelMenuRefresh: "Refresh",
      ariaLabelMenuFirstPage: "First page",
      ariaLabelMenuLastPage: "Last page",
      ariaLabelMenuPreviousPage: "Previous page",
      ariaLabelMenuNextPage: "Next page",
      ariaLabelMenuPageNumber: "Page number selector",
      ariaLabelMenuDocumentMap: "Toggle document map",
      ariaLabelMenuParametersArea: "Toggle parameters area",
      ariaLabelMenuZoomIn: "Zoom in",
      ariaLabelMenuZoomOut: "Zoom out",
      ariaLabelMenuPageState: "Toggle FullPage/PageWidth",
      ariaLabelMenuPrint: "Print",
      ariaLabelMenuContinuousScroll: "Continuous scrolling",
      ariaLabelMenuSendMail: "Send an email",
      ariaLabelMenuExport: "Export",
      ariaLabelMenuPrintPreview: "Toggle print preview",
      ariaLabelMenuSearch: "Search in report contents",
      ariaLabelMenuSideMenu: "Toggle side menu",
      ariaLabelSendEmailFrom: "From email address",
      ariaLabelSendEmailTo: "Recipient email address",
      ariaLabelSendEmailCC: "Carbon Copy email address",
      ariaLabelSendEmailSubject: "Email subject:",
      ariaLabelSendEmailFormat: "Report format:",
      ariaLabelSendEmailSend: "Send email",
      ariaLabelSendEmailCancel: "Cancel sending email",
      // search dialog resources
      searchDialogTitle: "Search in report contents",
      searchDialogSearchInProgress: "searching...",
      searchDialogNoResultsLabel: "No results",
      searchDialogResultsFormatLabel: "Result {0} of {1}",
      searchDialogStopTitle: "Stop Search",
      searchDialogNavigateUpTitle: "Navigate Up",
      searchDialogNavigateDownTitle: "Navigate Down",
      searchDialogMatchCaseTitle: "Match Case",
      searchDialogMatchWholeWordTitle: "Match Whole Word",
      searchDialogUseRegexTitle: "Use Regex",
      searchDialogCaptionText: "Find",
      searchDialogPageText: "page",
      // Send Email dialog resources
      sendEmailDialogTitle: "Send Email",
      sendEmailValidationEmailRequired: "Email field is required",
      sendEmailValidationEmailFormat: "Email format is not valid",
      sendEmailValidationSingleEmail: "The field accepts a single email address only",
      sendEmailValidationFormatRequired: "Format field is required",
      errorSendingDocument: "Error sending report document (Report = '{0}').",
      aiPromptConsentDialogTitle: "Before you start with AI",
      aiPromptConsentAgreeLabel: "Consent",
      aiPromptConsentRejectLabel: "Cancel"
    };
    window.telerikReportViewer ||= {};
    window.telerikReportViewer.sr ||= sr;

})();
