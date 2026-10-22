(() => {
    function reopenCreateUserPanel() {
        const panel = document.querySelector('#create-user-panel[data-reopen="true"]');

        if (panel && !panel.matches(':popover-open')) {
            panel.showPopover();
            panel.querySelector('[data-form-feedback]')?.focus();
        }
    }

    reopenCreateUserPanel();
    Blazor.addEventListener('enhancedload', reopenCreateUserPanel);
})();
