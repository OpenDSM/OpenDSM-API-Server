$("#logout-btn").on('click', () => {
    document.cookie = `auth_token=null; expires=${new Date("2000").toUTCString()};path=/`
    document.cookie = `auth_email=null; expires=${new Date("2000").toUTCString()};path=/`
    window.location.href = "/"
})

$("#save-profile-btn").on('click', async () => {
    $("#save-profile-btn")[0].disabled = true;
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
        let popup = new ImagePopup(e.currentTarget, file, 1 / 1, true, async base => {
            let data = new FormData();
            data.append("base64", base);
            data.append("email", email);
            data.append("token", token);
            await fetch('/api/auth/image/profile', { method: "POST", body: data })
            Array.from($(".profile-image")).forEach(item => {
                item.style.backgroundImage = `url("data:image/png;base64,${base}")`
            })
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
        let popup = new ImagePopup(e.currentTarget, file, 16 / 3.6667, false, async base => {
            let data = new FormData();
            data.append("base64", base);
            data.append("email", email);
            data.append("token", token);
            await fetch('/api/auth/image/banner', { method: "POST", body: data })
            $(".profile#landing")[0].style.backgroundImage = `url("data:image/png;base64,${base}")`
        });
        popup.open();
    })
    input.click();
})