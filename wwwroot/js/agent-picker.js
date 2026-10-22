// Prevent scrolling/submitting while the combobox uses these keys. Normal typing and Tab remain native.
document.addEventListener("keydown", event => {
    if (event.target instanceof HTMLInputElement && event.target.hasAttribute("data-agent-search") &&
        event.target.getAttribute("aria-expanded") === "true" &&
        ["ArrowUp", "ArrowDown", "Enter", "Escape"].includes(event.key)) {
        event.preventDefault();
    }
});
