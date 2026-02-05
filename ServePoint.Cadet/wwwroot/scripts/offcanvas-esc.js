window.offcanvasEsc = {
    register: function (dotNetRef) {
        function handler(e) {
            if (e.key === "Escape") {
                dotNetRef.invokeMethodAsync("OnEscPressed");
            }
        }

        document.addEventListener("keydown", handler);

        return {
            dispose: () => document.removeEventListener("keydown", handler)
        };
    }
};
