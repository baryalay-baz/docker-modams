// /wwwroot/js/pages/users/verifications/verifications-createschedule.js
(function (w, d, $) {
    "use strict";

    const AMS = w.AMS || (w.AMS = {});
    const U = AMS.util || (AMS.util = {});
    const NS = ".vcs"; // Verifications/CreateSchedule
    const PAGE_KEY = "Verifications/CreateSchedule";

    const SEL = {
        form: "#frmCreateSchedule",
        verifType: "#verificationTypeDropdown",
        numInStore: "#NumberOfAssets",
        numToVerify: "#NumberOfAssetsToVerify",
        spanNumErr: "#spanNumberOfAssetsToVerify",
        ddEmployees: "#dd-employees",
        ddRole: "#dd-role",
        addMember: ".btn-add-record",
        tblMembers: "#tblMembers #table-body",
        btnSubmit: "#btn-submit",
        hiddenTeam: "#teamMembersData",
        datePickers: ".picker"
    };

    const STATE = {
        team: [],
        counter: 0,
        isSo: (U.getCurrentLanguage?.() === "so"),
        employees: [] // normalized: [{ id, fullName, imageUrl }, ...]
    };

    // ---------- helpers ----------
    function hydrateEmployees() {
        // Priority 1: explicit config from the view (recommended)
        const cfg = w.VCS_CREATE_CONFIG || {};
        if (Array.isArray(cfg.employees) && cfg.employees.length) {
            STATE.employees = cfg.employees;
            return;
        }

        // Priority 2: global sData (can be string OR already an array/object)
        // eslint-disable-next-line no-undef
        if (typeof sData !== "undefined") {
            try {
                if (typeof sData === "string") {
                    const parsed = U.tryParseJson(sData, "Employees");
                    STATE.employees = parsed.status === "success" ? parsed.data : [];
                } else if (Array.isArray(sData)) {
                    STATE.employees = sData;
                } else if (sData && typeof sData === "object" && Array.isArray(sData.data)) {
                    STATE.employees = sData.data;
                }
            } catch {
                STATE.employees = [];
            }
        }
    }

    function getEmployeeById(id) {
        return STATE.employees.find(e => String(e.id) === String(id));
    }

    function getEmployeeLabel(employeeId) {
        const emp = getEmployeeById(employeeId);
        if (!emp) return "";
        return `
      <span class="avatar avatar-sm rounded-circle cover-image">
        <img src="${emp.imageUrl}" alt="${U.escapeHtml(emp.fullName)}" class="avatar avatar-sm rounded-circle">
      </span>
      <span class="employee-name">${U.escapeHtml(emp.fullName)}</span>
    `;
    }

    function initVendors() {
        if ($?.fn?.select2) {
            $(".select2").select2();
            // Revalidate when user changes a select2 field
            $(SEL.ddEmployees + "," + SEL.ddRole).on("change" + NS, function () {
                const $form = $(SEL.form);
                if ($ && $.validator && $form.data("validator")) {
                    $form.valid();
                }
            });
        }
        if ($?.fn?.datepicker) {
            $(SEL.datePickers).datepicker({
                autoclose: true,
                format: "dd-MM-yyyy",
                todayHighlight: true
            }).on("changeDate" + NS, function () {
                const $form = $(SEL.form);
                if ($ && $.validator && $form.data("validator")) {
                    $form.valid();
                }
            });
        } else {
            // graceful fallback: native date if you want
            $(SEL.datePickers).each(function () {
                try { if (this.type !== "date") this.type = "date"; } catch { }
            });
        }
    }

    function enableUnobtrusiveValidation() {
        // jQuery Validate ignores hidden inputs by default; Select2 hides the original <select>.
        // Make the validator include hidden inputs so rules on the original selects still run.
        if ($ && $.validator) {
            $.validator.setDefaults({ ignore: [] });
        }
        if ($ && $.validator && $.validator.unobtrusive) {
            const $form = $(SEL.form);
            // Parse adapters for this form (safe to call more than once)
            $.validator.unobtrusive.parse($form);
            // Double-ensure ignore is empty on this instance
            const v = $form.data("validator");
            if (v && v.settings) v.settings.ignore = "";
        }
    }

    function bindEvents() {
        U.on(NS, "click", SEL.addMember, onAddMember);
        U.on(NS, "change", SEL.verifType, onVerificationTypeChange);
        U.on(NS, "click", SEL.btnSubmit, onSubmitClick);
        U.on(NS, "click", `${SEL.tblMembers} a[data-id]`, onRemoveClick);
    }

    // ---------- handlers ----------
    function onAddMember() {
        const employeeId = $(SEL.ddEmployees).val();
        const employeeName = $(SEL.ddEmployees + " option:selected").text();
        const role = $(SEL.ddRole).val();
        if (!employeeId || employeeId === "0" || !role) return;

        STATE.counter += 1;
        const rowId = STATE.counter;

        // push normalized team row (we'll only persist id+role+name)
        STATE.team.push({ id: rowId, employeeId, employeeName, role });

        // Avatar label (same look as EditSchedule)
        const emp = getEmployeeById(employeeId);
        const label = emp
            ? getEmployeeLabel(employeeId)
            : U.escapeHtml(employeeName);

        const localized = {
            "Team Leader": STATE.isSo ? "Hoggaamiyaha Kooxda" : "Team Leader",
            "Team Member": STATE.isSo ? "Xubin Koox" : "Team Member"
        };

        $(SEL.tblMembers).append(
            `<tr id="member-${rowId}">
        <td class="text-black bg-transparent border-bottom-0 w-2">${rowId}</td>
        <td class="text-black bg-transparent border-bottom-0 w-20">${label}</td>
        <td class="text-black bg-transparent border-bottom-0 w-10">${localized[role] || role}</td>
        <td class="text-black bg-transparent border-bottom-0 w-5 no-btn">
          <a style="cursor:pointer;" data-id="${rowId}"><i class="fa fa-times text-danger"></i></a>
        </td>
      </tr>`
        );

        // de-dup the chosen employee & TL option
        $(`${SEL.ddEmployees} option[value='${employeeId}']`).remove();
        if (role === "Team Leader") {
            $(`${SEL.ddRole} option[value='Team Leader']`).remove();
        }
    }

    function onRemoveClick() {
        const id = Number(this.getAttribute("data-id"));
        if (!Number.isFinite(id)) return;

        const idx = STATE.team.findIndex(m => m.id === id);
        if (idx === -1) return;

        const removed = STATE.team[idx];

        // restore employee in dropdown
        if (removed) {
            $(SEL.ddEmployees).append(
                `<option value="${removed.employeeId}">${U.escapeHtml(removed.employeeName)}</option>`
            );
            if (removed.role === "Team Leader" && !$(SEL.ddRole + ' option[value="Team Leader"]').length) {
                const tlLabel = STATE.isSo ? "Hoggaamiyaha Kooxda" : "Team Leader";
                $(SEL.ddRole).append(`<option value="Team Leader">${tlLabel}</option>`);
            }
        }

        STATE.team.splice(idx, 1);
        $(`#member-${id}`).remove();
    }

    function onVerificationTypeChange() {
        const assetCount = U.toInt($(SEL.numInStore).val(), 0);
        const selected = $(SEL.verifType).val();
        if (selected === "Custom Verification") {
            $(SEL.numToVerify).val(0).prop("readonly", false);
        } else {
            $(SEL.numToVerify).val(assetCount).prop("readonly", true);
        }
    }

    function onSubmitClick() {
        const $btn = $(SEL.btnSubmit);
        const $form = $(SEL.form);
        const origCl = $btn.attr("class");
        const origHt = $btn.html();

        function setLoading() { $btn.addClass("btn-loading btn-icon").prop("disabled", true); }
        function resetBtn() { $btn.attr("class", origCl).html(origHt).prop("disabled", false); }

        // 🔹 1) Run client-side unobtrusive validation first
        if ($ && $.validator && $.validator.unobtrusive) {
            // In case dynamic elements appeared, ensure parse is applied
            $.validator.unobtrusive.parse($form);
            if (!$form.valid()) return; // plugin will show messages next to inputs
        }

        resetBtn();

        // 🔹 2) Your custom numeric check (range check)
        const assetsToVerify = U.toInt($(SEL.numToVerify).val(), NaN);
        const assetCount = U.toInt($(SEL.numInStore).val(), 0);
        let ok = true;

        if (!Number.isInteger(assetsToVerify) || assetsToVerify < 1 || assetsToVerify > assetCount) {
            $(SEL.spanNumErr).text(
                STATE.isSo ? `Qiimaha waa inuu u dhexeeyaa 1 iyo ${assetCount}`
                    : `Value should be between 1 and ${assetCount}`
            );
            ok = false;
        } else {
            $(SEL.spanNumErr).text("");
        }

        // 🔹 3) Team validation
        if (!ok || STATE.team.length === 0) {
            if (STATE.team.length === 0 && typeof w.notif === "function") {
                w.notif({
                    type: "error",
                    msg: STATE.isSo ? "<b>Khalad: </b>Xubno Kooxeed lama xulin!"
                        : "<b>Error: </b>No Team members selected!",
                    position: "center", width: 500, height: 60, autohide: true
                });
            }
            resetBtn();
            return;
        }

        // 🔹 4) Submit
        $(SEL.hiddenTeam).val(JSON.stringify(STATE.team));
        setLoading();
        try { $form[0].submit(); } catch { resetBtn(); }
    }

    // ---------- init ----------
    function init() {
        // avoid double init
        if (d.documentElement.dataset.vcsInit === "1") return;
        d.documentElement.dataset.vcsInit = "1";

        hydrateEmployees();
        initVendors();
        enableUnobtrusiveValidation(); // <-- key line
        bindEvents();
    }

    // ---- PAGE-REGISTRY (your pattern) ----
    AMS.pages?.register?.(PAGE_KEY, init);

    // Fallback auto-init if registry isn't present yet
    const currentKey = U.pageKey?.() || d.body.getAttribute("data-page") || "";
    if (currentKey === PAGE_KEY && !(AMS.pages?.register)) {
        U.ready(init);
    }

})(window, document, window.jQuery);
