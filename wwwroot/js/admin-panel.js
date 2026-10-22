(() => {
    function reopenCreateUserPanel() {
        for (const panel of document.querySelectorAll('.create-user-drawer[data-reopen="true"]')) {
            if (!panel.matches(':popover-open')) {
                panel.showPopover();
                panel.querySelector('[data-form-feedback]')?.focus();
            }
        }
    }

    reopenCreateUserPanel();
    Blazor.addEventListener('enhancedload', reopenCreateUserPanel);
})();
