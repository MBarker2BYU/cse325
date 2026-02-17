(() => {
    // Print
    window.spPrint = () => window.print();

    // Download (reliable: Blob + object URL)
    window.spDownloadFile = (fileName, contentType, base64Data) => {
        try {
            const safeName = fileName || "download";
            const type = contentType || "application/octet-stream";

            // base64 -> bytes
            const binary = atob(base64Data);
            const bytes = new Uint8Array(binary.length);
            for (let i = 0; i < binary.length; i++) {
                bytes[i] = binary.charCodeAt(i);
            }

            const blob = new Blob([bytes], { type });
            const url = URL.createObjectURL(blob);

            const a = document.createElement("a");
            a.style.display = "none";
            a.href = url;
            a.download = safeName;

            document.body.appendChild(a);
            a.click();
            a.remove();

            // cleanup
            setTimeout(() => URL.revokeObjectURL(url), 1000);
        } catch (e) {
            console.error("spDownloadFile failed:", e);
            alert("Download failed. Check the browser console for details.");
        }
    };
})();
