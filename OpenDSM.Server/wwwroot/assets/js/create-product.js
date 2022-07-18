$("#use-readme-toggle").on('click', e => {
    let value = $(e.currentTarget).attr("value") == "true";
    $('#about-input')[0].style.display = value ? '' : 'none'
})



$("#upload-icon.file-upload").on('click', e => {
    let input = document.createElement('input');
    input.type = "file"
    input.accept = "image/*";
    $(input).on('change', l => {
        let file = l.currentTarget.files[0];
        let popup = new ImagePopup(file, 1 / 1, true, async () => {
            let data = new FormData();
            data.append("base64", $(this.image).data('cropper').getCroppedCanvas().toDataURL().split('base64,')[1]);
            data.append("email", this.email);
            data.append("token", this.token);
            await fetch('/api/auth/image/profile', { method: "POST", body: data })

            window.location.reload();
        });
        popup.open();
    })
    input.click();
})