// /wwwroot/js/pages/users/verifications/verifications-editschedule.js
(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});

    const SEL = {
        form: "#frmEditSchedule",
        store: "#storeDropdown",
        type: "#verificationTypeDropdown",
        numInStore: "#NumberOfAssets",
        numToVerify: "#NumberOfAssetsToVerify",
        spanNumToVerify: "#spanNumberOfAssetsToVerify",
        ddEmployees: "#dd-employees",
        ddRole: "#dd-role",
        btnAddMember: ".btn-add-record",
        tblBody: "#table-body",
        btnSubmit: "#btn-submit",
        btnDelete: "#btnDeleteSchedule",
        datePickers: ".picker" // <- your date inputs
    };

    const STATE = {
        members: [],
        counter: 0,
        isSomali: (typeof window.getCurrentLanguage === "function" ? window.getCurrentLanguage() : "en") === "so",
        employees: [],
        editTeam: [],
        scheduleId: null,
        deleteUrl: null,
        i18n: {
            deleteTitle: "Delete Schedule",
            deleteMessage: "Are you sure you want to delete this schedule?",
            confirm: "Confirm",
            cancel: "Cancel"
        }
    };

    function hydrateConfig() {
        const cfg = window.VES_EDIT_CONFIG || {};
        if (Array.isArray(cfg.employees)) STATE.employees = cfg.employees;
        if (Array.isArray(cfg.editTeam)) STATE.editTeam = cfg.editTeam;
        if (cfg.scheduleId != null) STATE.scheduleId = cfg.scheduleId;
        if (cfg.deleteUrl) STATE.deleteUrl = cfg.deleteUrl;

        if (cfg.i18n && typeof cfg.i18n === "object") STATE.i18n = Object.assign({}, STATE.i18n, cfg.i18n);
        if (STATE.isSomali) STATE.i18n = Object.assign({}, STATE.i18n, {
            deleteTitle: "Tirtir Jadwalka",
            deleteMessage: "Ma hubtaa inaad tirtirto jadwalkan?",
            confirm: "Xaqiiji",
            cancel: "Jooji"
        });

        // legacy fallbacks
        if (!STATE.employees.length && window.sData) {
            try { STATE.employees = Array.isArray(window.sData) ? window.sData : JSON.parse(window.sData); } catch { }
        }
        if (!STATE.editTeam.length && window.currentTeamData) {
            try { STATE.editTeam = Array.isArray(window.currentTeamData) ? currentTeamData : JSON.parse(window.currentTeamData); } catch { }
        }
    }

    function getEmployeeLabel(employeeId) {
        const emp = STATE.employees.find(e => String(e.id) === String(employeeId));
        if (!emp) return "";
        return `
      <span class="avatar avatar-sm rounded-circle cover-image">
        <img src="${emp.imageUrl}" alt="${U.escapeHtml(emp.fullName)}" class="avatar avatar-sm rounded-circle">
      </span>
      <span class="employee-name">${U.escapeHtml(emp.fullName)}</span>
    `;
    }

    function addMemberRow(employeeId, role) {
        STATE.counter++;
        const id = STATE.counter;

        STATE.members.push({ id, employeeId, role });

        const localizedRole =
            role === "Team Leader"
                ? (STATE.isSomali ? "Hoggaamiyaha Kooxda" : "Team Leader")
                : (STATE.isSomali ? "Xubin Koox" : "Team Member");

        const rowHtml = `
      <tr id="member-${id}">
        <td class="text-black bg-transparent border-bottom-0 w-2">${id}</td>
        <td class="text-black bg-transparent border-bottom-0 w-20">${getEmployeeLabel(employeeId)}</td>
        <td class="text-black bg-transparent border-bottom-0 w-10">${localizedRole}</td>
        <td class="text-black bg-transparent border-bottom-0 w-5 no-btn">
          <a style="cursor:pointer;" data-remove-id="${id}">
            <i class="fa fa-times text-danger"></i>
          </a>
        </td>
      </tr>
    `;
        $(SEL.tblBody).append(rowHtml);

        $(`${SEL.ddEmployees} option[value='${employeeId}']`).remove();
        if (role === "Team Leader") {
            $(`${SEL.ddRole} option[value='Team Leader']`).remove();
        }
    }

    function removeMemberRow(id) {
        const item = STATE.members.find(m => m.id === id);
        if (!item) return;

        $(`#member-${id}`).remove();

        if (item.role === "Team Leader" && !$(SEL.ddRole).find("option[value='Team Leader']").length) {
            const labelTL = STATE.isSomali ? "Hoggaamiyaha Kooxda" : "Team Leader";
            $(SEL.ddRole).append(`<option value="Team Leader">${labelTL}</option>`);
        }

        const emp = STATE.employees.find(e => String(e.id) === String(item.employeeId));
        if (emp) $(SEL.ddEmployees).append(`<option value="${emp.id}">${U.escapeHtml(emp.fullName)}</option>`);

        STATE.members = STATE.members.filter(m => m.id !== id);
    }

    function hydrateExistingTeam() {
        if (!Array.isArray(STATE.editTeam)) return;
        STATE.editTeam.forEach(m => addMemberRow(m.employeeId, m.role));
    }

    function initSelect2() {
        if ($.fn && $.fn.select2) {
            $(".select2").select2();
        } else {
            console.warn("[VES] select2 not found; continuing without it.");
        }
    }

    // 🔧 SAFE datepicker init (won’t crash if plugin missing)
    function initDatePickers() {
        const $inputs = $(SEL.datePickers);
        if (!$inputs.length) return;

        if ($.fn && $.fn.datepicker) {
            $inputs.datepicker({
                autoclose: true,
                format: "dd-MM-yyyy",
                todayHighlight: true
            });
        } else {
            console.warn("[VES] .datepicker() not found. Falling back to native date inputs.");
            // fallback: try native date; keep your formatting UX reasonable
            $inputs.each(function () {
                // If it's not already a date input, switch to date
                if (this.type !== "date") {
                    // If you must keep text, you can skip changing type and just leave it.
                    // But native date is the quickest usable fallback:
                    try { this.type = "date"; } catch { }
                }
            });
        }
    }

    function syncNumberOfAssets() {
        const assetCount = $(SEL.store).find("option:selected").data("asset-count");
        $(SEL.numInStore).val(assetCount);
        const type = $(SEL.type).val();
        if (type === "Custom Verification") {
            $(SEL.numToVerify).prop("readonly", false);
        } else {
            $(SEL.numToVerify).val(assetCount).prop("readonly", true);
        }
    }

    function syncForTypeChange() {
        const assetCount = $(SEL.store).find("option:selected").data("asset-count");
        const type = $(SEL.type).val();
        if (type === "Custom Verification") {
            $(SEL.numToVerify).prop("readonly", false);
        } else {
            $(SEL.numToVerify).val(assetCount).prop("readonly", true);
        }
    }

    function validateBeforeSave() {
        const assetsToVerify = parseInt($(SEL.numToVerify).val(), 10);
        const assetCount = parseInt($(SEL.store).find("option:selected").data("asset-count"), 10) || 0;

        if (!Number.isInteger(assetsToVerify) || assetsToVerify < 1 || assetsToVerify > assetCount) {
            const msg = STATE.isSomali
                ? `Qiimaha waa inuu u dhexeeyaa 1 iyo ${assetCount}`
                : `Value should be between 1 and ${assetCount}`;
            $(SEL.spanNumToVerify).text(msg);
            return false;
        }
        $(SEL.spanNumToVerify).text("");

        if (STATE.members.length === 0) {
            if (typeof window.notif === "function") {
                window.notif({
                    type: "error",
                    msg: STATE.isSomali
                        ? "<b>Khalad: </b>Xubno Kooxeed lama xulin!"
                        : "<b>Error: </b>No Team members selected!",
                    position: "center",
                    width: 500, height: 60, autohide: true
                });
            }
            return false;
        }
        return true;
    }

    function resolveDeleteUrl() {
        const dataUrl = $(SEL.btnDelete).data("delete-url");
        if (dataUrl) return dataUrl;
        if (STATE.deleteUrl) return STATE.deleteUrl;
        if (STATE.scheduleId != null) return `/Users/Verifications/DeleteSchedule/${encodeURIComponent(STATE.scheduleId)}`;
        return null;
    }

    function init() {
        try {
            hydrateConfig();
            initSelect2();
            initDatePickers();

            hydrateExistingTeam();
            syncNumberOfAssets();

            const NS = ".vesEdit";
            U.on(NS, "change", SEL.store, syncNumberOfAssets);
            U.on(NS, "change", SEL.type, syncForTypeChange);

            U.on(NS, "click", SEL.btnAddMember, function () {
                const empId = $(SEL.ddEmployees).val();
                const role = $(SEL.ddRole).val();
                if (!empId || empId === "0" || !role) return;
                addMemberRow(empId, role);
            });

            U.on(NS, "click", `${SEL.tblBody} [data-remove-id]`, function () {
                const id = parseInt($(this).data("remove-id"), 10);
                if (Number.isInteger(id)) removeMemberRow(id);
            });

            U.on(NS, "click", SEL.btnSubmit, function () {
                if (!validateBeforeSave()) return;

                $("#teamMembersData").val(JSON.stringify(
                    STATE.members.map(m => ({ id: m.id, employeeId: m.employeeId, role: m.role }))
                ));
                const formEl = document.querySelector(SEL.form);
                if (formEl) formEl.submit();
            });

            U.on(NS, "click", SEL.btnDelete, function (e) {
                e.preventDefault();
                const url = resolveDeleteUrl();
                if (!url) {
                    console.error("[VES] Delete URL not set.");
                    return;
                }
                if (typeof window.openConfirmation === "function") {
                    openConfirmation({
                        actionUrl: url,
                        title: STATE.i18n.deleteTitle,
                        message: STATE.i18n.deleteMessage,
                        btnConfirmText: STATE.i18n.confirm,
                        btnCancelText: STATE.i18n.cancel
                    });
                } else {
                    if (window.confirm(STATE.i18n.deleteMessage)) {
                        const form = document.createElement("form");
                        form.method = "POST";
                        form.action = url;
                        const token =
                            document.querySelector('input[name="__RequestVerificationToken"]')?.value ||
                            document.querySelector('meta[name="request-verification-token"]')?.content;
                        if (token) {
                            const inp = document.createElement("input");
                            inp.type = "hidden";
                            inp.name = "__RequestVerificationToken";
                            inp.value = token;
                            form.appendChild(inp);
                        }
                        document.body.appendChild(form);
                        form.submit();
                    }
                }
            });
        } catch (err) {
            console.error("[AMS] Error in Verifications/EditSchedule init:", err);
        }
    }

    AMS.pages?.register?.("Verifications/EditSchedule", init);
    if (!AMS.pages || !AMS.pages.register) {
        U.ready(init);
    }
})(jQuery, window, document);
