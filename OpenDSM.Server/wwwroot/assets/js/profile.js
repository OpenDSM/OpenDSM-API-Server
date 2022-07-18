$("#logout-btn").on('click', () => {
    document.cookie = `auth_token=null; expires=${new Date("2000").toUTCString()};path=/`
    document.cookie = `auth_email=null; expires=${new Date("2000").toUTCString()};path=/`
    window.location.href = "/"
})

$("#save-profile-btn").on('click', async () => {
    $("#save-profile-btn")[0].disabled = true;
    let email;
    let token;

    Array.from(document.cookie.split(';')).forEach(item => {
        let key = item.split("=")[0].trim()
        if (key == "auth_email") {
            email = item.replace(key + "=", "");
        }
        if (key == "auth_token") {
            token = item.replace(key + "=", "");
        }
    })
    Array.from($("[setting][modified]")).forEach(async e => {
        let name = $(e).attr('setting');
        let value = e.value;
        let data = new FormData();
        data.append("name", name);
        data.append("value", value);
        data.append("email", email);
        data.append("token", token);
        await fetch("/api/auth/settings", { method: "PATCH", body: data })
    })
})

$("#about-box").on('keyup', e => {
    updateAbout(e.currentTarget.value)
})
updateAbout($("#about-rendering")[0].innerHTML)
function updateAbout(value) {
    let converter = new showdown.Converter()
    $("#about-rendering")[0].innerHTML = converter.makeHtml(value);
}

$("[setting]").on('keyup', e => {
    $("#save-profile-btn")[0].disabled = false;
    if (e.key == "Enter") {
        $("#save-profile-btn")[0].click();
    }
    $(e.currentTarget).attr("modified", "")
})

$("#upload-profile-image.file-upload").on('click', e => {
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

$("#upload-profile-banner.file-upload").on('click', e => {
    let input = document.createElement('input');
    input.type = "file"
    input.accept = "image/*";
    $(input).on('change', l => {
        let file = l.currentTarget.files[0];
        let popup = new ImagePopup(file, 16 / 3.6667, false, async () => {
            let data = new FormData();
            data.append("base64", $(this.image).data('cropper').getCroppedCanvas().toDataURL().split('base64,')[1]);

            data.append("email", this.email);
            data.append("token", this.token);
            await fetch('/api/auth/image/banner', { method: "POST", body: data })
            window.location.reload();
        });
        popup.open();
    })
    input.click();
})