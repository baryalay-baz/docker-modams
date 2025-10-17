// wwwroot/js/tours/assets-assetinfo-tour.js
(function () {
    const PAGE_KEY = "Assets/AssetInfo";
    const H = (window.PAMS_PAGE && window.PAMS_PAGE.AssetInfo) || {};
    const { forceShowTab, activatePaneForElement, RESUME } = H;

    // i18n
    const lang = (window.getCurrentLanguage && window.getCurrentLanguage()) || "en";
    const t = (en, so) => (lang === "so" ? so : en);

    // ---- Tunables ----
    const TAB_SWITCH_DELAY_MS = 150;     // small delay for smoother tab switches
    const RESUME_ENABLED = true;    // set false to disable resume

    // Utility: delayed tab switch
    const delayedSwitch = (sel, ms = TAB_SWITCH_DELAY_MS) =>
        setTimeout(() => forceShowTab && forceShowTab(sel), ms);

    // Helper to create a tab-switcher step
    function switchTabStep(linkSelector, titleEn, titleSo, descEn, descSo) {
        const activate = () => delayedSwitch(linkSelector);
        return {
            element: linkSelector,
            popover: {
                title: t(titleEn, titleSo),
                description: t(descEn, descSo)
            },
            onHighlighted: activate,
            onNextClick: (el, step, driver) => { activate(); setTimeout(() => driver?.moveNext?.(), TAB_SWITCH_DELAY_MS); }
        };
    }

    // ===== TAB 1 (exact order) =====
    const TAB1 = [
        { element: '[data-tour="ai.make"]', popover: { title: t('Make / Manufacturer', 'Soo-saaraha'), description: t('Brand or manufacturer of the asset.', 'Calaamadda ama shirkadda soo saartay hantidan.') } },
        { element: '[data-tour="ai.model"]', popover: { title: t('Model', 'Nooca / Qaabka'), description: t('Specific model or version of the asset.', 'Nooca ama lambarka qaabka ee hantidan.') } },
        { element: '[data-tour="ai.year"]', popover: { title: t('Year of Manufacture', 'Sannadka Soo-saaridda'), description: t('The year this asset was manufactured or released.', 'Sannadka la soo saaray ama la sii daayay hantidan.') } },

        { element: '[data-tour="ai.assetname.row"]', popover: { title: t('Asset Name', 'Magaca Hantida'), description: t('Official name or title of the asset.', 'Magaca ama cinwaanka rasmiga ah ee hantida.') } },
        { element: '[data-tour="ai.country"]', popover: { title: t('Country of Manufacture', 'Dalka Soo-saaray'), description: t('The country where this asset was manufactured.', 'Dalka lagu soo saaray hantidan.') } },
        { element: '[data-tour="ai.category"]', popover: { title: t('Asset Category', 'Qaybta Hantida'), description: t('Select the main category that best represents the asset.', 'Dooro qaybta ugu habboon ee si fiican u metelaysa hantidan.') } },

        { element: '[data-tour="ai.serialno"]', popover: { title: t('Serial Number', 'Lambarka Taxanaha'), description: t('Unique serial number for this asset (if available).', 'Lambarka taxanaha ee gaarka ah ee hantidan (haddii uu jiro).') } },
        { element: '[data-tour="ai.barcode"]', popover: { title: t('Barcode', 'Koodhka Baarka'), description: t('Scan or enter the barcode assigned to the asset.', 'Iskaani ama geli koodhka baarka ee hantidan loo qoondeeyay.') } },
        { element: '[data-tour="ai.po"]', popover: { title: t('Purchase Order Number', 'Lambarka Amar Iibsi'), description: t('Purchase Order number linked to this asset.', 'Lambarka amarka iibka ee la xidhiidha hantidan.') } },

        { element: '[data-tour="ai.purchasedate"]', popover: { title: t('Purchase Date', 'Taariikhda Iibsiga'), description: t('The date the asset was purchased.', 'Taariikhda la iibsaday hantidan.') } },
        { element: '[data-tour="ai.receiptdate"]', popover: { title: t('Receipt Date', 'Taariikhda Lagu Helay'), description: t('The date the organization received the asset.', 'Taariikhda uu ururku hantidan gacanta ku dhigay.') } },
        { element: '[data-tour="ai.procuredby"]', popover: { title: t('Procured By', 'Cidda Soo Iibsaday'), description: t('Select who procured this asset (e.g., UNOPS, EU, Somalia).', 'Dooro cidda soo iibsatay hantidan (tusaale: UNOPS, EU, Somalia).') } },

        { element: '[data-tour="ai.condition"]', popover: { title: t('Condition', 'Xaaladda Hantida'), description: t('Current condition (e.g., New, Good, Damaged).', 'Xaaladda hadda (tusaale: Cusub, Wanaagsan, Jaban).') } },
        { element: '[data-tour="ai.donor"]', popover: { title: t('Donor', 'Deeq-bixiye'), description: t('Donor that funded or contributed this asset.', 'Deeq-bixiyaha maalgeliyay ama ku deeqay hantidan.') } },
        { element: '[data-tour="ai.status"]', popover: { title: t('Status', 'Xaalad'), description: t('Current status of the asset.', 'Xaaladda hadda ee hantida.') } },

        { element: '[data-tour="ai.specifications"]', popover: { title: t('Specifications', 'Faahfaahinta Hantida'), description: t('Key technical or descriptive specifications of the asset.', 'Faahfaahinta farsamo ama sharaxaad ee muhiimka ah ee hantidan.') } },

        { element: '[data-tour="ai.engine"]', popover: { title: t('Engine Number', 'Lambarka Injinka'), description: t('Unique identifier for the vehicle engine.', 'Aqoonsiga gaarka ah ee injinka gaadiidka.') } },
        { element: '[data-tour="ai.chasis"]', popover: { title: t('Chasis Number', 'Lambarka Chasiska'), description: t('Vehicle identification number.', 'Aqoonsiga gaarka ah ee gaadiidka.') } },
        { element: '[data-tour="ai.plate"]', popover: { title: t('Plate Number', 'Lambarka Taarikada'), description: t('Registration plate number.', 'Lambarka taarikada diiwaangelinta.') } },

        { element: '[data-tour="ai.remarks"]', popover: { title: t('Remarks', 'Faallooyin'), description: t('Add any relevant notes or comments about this asset.', 'Ku dar qoraallo ama faallooyin khuseeya hantidan.') } },
        { element: '[data-tour="ai.delete"]', popover: { title: t('Delete Asset', 'Tirtir Hantida'), description: t('Delete this asset (administrators only).', 'Ka tirtir hantidan (kaliya maamulayaasha).') } },

        { element: '[data-tour="ai.cost"]', popover: { title: t('Cost', 'Qiimaha'), description: t('Total cost or purchase price of the asset.', 'Qiimaha guud ama lacagta lagu iibsaday hantidan.') } },
        { element: '[data-tour="ai.usage"]', popover: { title: t('Asset Usage', 'Isticmaalka Hantida'), description: t('Usage to date as a proportion of total lifespan.', 'Isticmaalka ilaa hadda marka loo eego cimriga guud.') } },
        { element: '[data-tour="ai.currentvalue"]', popover: { title: t('Current Value', 'Qiimaha Hadda'), description: t('Estimated current value after depreciation.', 'Qiyaasta qiimaha hadda ka dib dhimista (depreciation).') } },

        { element: '[data-tour="ai.assethistory"]', popover: { title: t('Asset History', 'Taariikhda Hantida'), description: t('Event timeline since registration (transfers, handovers, etc.).', 'Jadwalka dhacdooyinka tan iyo diiwaangelinta (wareejinno, iwm).') } }
    ];

    // Explicit switches (they’ll be auto-activated by callbacks)
    const SWITCH_TO_TAB2 = [
        switchTabStep(
            "#a_tab_2",
            "Open Documents", "Fur Dukumentiyada",
            "Switching to the Documents tab to continue the tour.",
            "Waxaa lagu wareegayaa tabka Dukumentiyada si socdaalka loo sii wado."
        )
    ];

    const TAB2 = [
        { element: '[data-tour="ai.tab2"]', popover: { title: t('Documents Area', 'Aagga Dukumentiyada'), description: t('This tab lists all associated documents for quick access.', 'Tabkani waxa uu soo bandhigaa dhammaan dukumentiyada la xidhiidha si degdeg ah loogu helo.') } },
        { element: '[data-tour="ai.documents.header"]', popover: { title: t('Documents Header', 'Cinwaanka Dukumentiyada'), description: t('Title and quick actions for managing the asset’s documents.', 'Cinwaan iyo falal degdeg ah oo lagu maamulo dukumentiyada hantidan.') } },
        { element: '[data-tour="ai.documents.edit"]', popover: { title: t('Edit Documents', 'Tafatir Dukumentiyada'), description: t('Administrators can add, replace, or remove files here.', 'Maamulayaashu halkan waxay ka dari karaan, beddeli karaan ama ka saari karaan faylasha.') } },
        { element: '[data-tour="ai.documents.table"]', popover: { title: t('Documents Table', 'Jadwalka Dukumentiyada'), description: t('Each row shows a document’s name and its download link.', 'Saf kasta waxa uu muujinayaa magaca dukumentiga iyo xiriirka soo dejinta.') } },
        { element: '[data-tour="ai.documents.rowname"]', popover: { title: t('Document Name', 'Magaca Dukumentiga'), description: t('The official name/type of the document associated with the asset.', 'Magaca/nuuca rasmiga ah ee dukumentiga la xidhiidha hantidan.') } },
        { element: '[data-tour="ai.documents.download"]', popover: { title: t('Download File', 'Soo Dejiso Faylka'), description: t('Click to download the selected document.', 'Guji si aad u soo dejiso dukumentiga la doortay.') } }
    ];

    const SWITCH_TO_TAB3 = [
        switchTabStep(
            "#a_tab_3",
            "Open Pictures", "Fur Sawirrada",
            "Switching to the Pictures tab to finish the tour.",
            "Waxaa lagu wareegayaa tabka Sawirrada si socdaalka loo dhamaystiro."
        )
    ];

    const TAB3 = [
        { element: '[data-tour="ai.tab3"]', popover: { title: t('Gallery Area', 'Aagga Sawirrada'), description: t('All uploaded photos for the asset are displayed here.', 'Dhammaan sawirrada la soo geliyey ee hantidan halkan ayaa lagu soo bandhigayaa.') } },
        { element: '[data-tour="ai.gallery.header"]', popover: { title: t('Gallery Header', 'Cinwaanka Sawirrada'), description: t('Manage or update the photo gallery if you have permission.', 'Haddii aad haysato oggolaansho, halkan ka maamul ama cusboonaysii sawirrada.') } },
        { element: '[data-tour="ai.gallery.body"]', popover: { title: t('Gallery Grid', 'Shabkadda Sawirrada'), description: t('Thumbnails are shown in a responsive grid. Click to view full size.', 'Sawirrada yaryar ayaa lagu soo bandhigayaa shabakad la qabsata. Guji si aad u aragto buuxa.') } },
        { element: '[data-tour="ai.gallery.item-1"]', popover: { title: t('Photo Item', 'Sawir'), description: t('Open this image in the lightbox to inspect details.', 'Fur sawirkan sanduuqa muuqaalka si faahfaahin loo arko.') } },
        { element: '[data-tour="ai.gallery.pagination"]', popover: { title: t('Pagination', 'Qaybinta Bogagga'), description: t('Use pagination to navigate between picture pages.', 'Isticmaal qaybinta bogagga si aad u dhex marto bogagga sawirrada.') } }
    ];

    // Combine
    let ALL_STEPS = [
        ...TAB1,
        ...SWITCH_TO_TAB2,
        ...TAB2,
        ...SWITCH_TO_TAB3,
        ...TAB3
    ];

    // --- Auto-skip steps whose elements don't exist (e.g., vehicle-only) ---
    ALL_STEPS = ALL_STEPS.filter(s => {
        try { return !!document.querySelector(s.element); } catch { return false; }
    });

    // --- Inject index-aware callbacks for RESUME + pane activation ---
    const FINAL_STEPS = ALL_STEPS.map((s, idx) => {
        const wrap = (fn) => (el, step, driver) => {
            // Save progress (resume)
            if (RESUME_ENABLED) RESUME?.save?.(idx);

            // Ensure its pane is visible (prevents popover at 0,0)
            try { activatePaneForElement?.(el); } catch (_) { }

            // Call original if provided
            if (typeof fn === 'function') fn(el, step, driver);

            // Resume jump logic only on FIRST step
            if (RESUME_ENABLED && idx === 0) {
                const target = Math.max(0, RESUME?.read?.() || 0);
                if (target > 0) {
                    // fast-forward by calling moveNext repeatedly
                    let hops = target;
                    const hop = () => {
                        if (hops-- <= 0) return;
                        driver?.moveNext?.();
                        setTimeout(hop, 0);
                    };
                    setTimeout(hop, 0);
                }
            }
        };

        return {
            ...s,
            onHighlighted: wrap(s.onHighlighted),
            onNextClick: s.onNextClick // keep any per-step next behavior (e.g., switchTabStep)
        };
    });

    // Register for your loader
    window.PAMS_TOUR_REGISTRY = window.PAMS_TOUR_REGISTRY || {};
    window.PAMS_TOUR_REGISTRY[PAGE_KEY] = { steps: FINAL_STEPS, version: "v1-polished" };
})();
