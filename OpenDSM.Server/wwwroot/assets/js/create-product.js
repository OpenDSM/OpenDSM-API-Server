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
        let popup = new ImagePopup(e.currentTarget, file, 1 / 1, false, async base => { });
        popup.open();
    })
    input.click();
})
$("#upload-banner.file-upload").on('click', e => {
    let input = document.createElement('input');
    input.type = "file"
    input.accept = "image/*";
    $(input).on('change', l => {
        let file = l.currentTarget.files[0];
        let popup = new ImagePopup(e.currentTarget, file, 16 / 9, false, async base => { });
        popup.open();
    })
    input.click();
})

$("#yt-key-box").on('focusin', async e => {
    try {
        let clipboard = await navigator.clipboard.readText();
        if (clipboard.includes("youtube.com/watch?v=")) {
            let key = clipboard.split("watch?v=")[1].split("&")[0].replace("watch?v=", "").replace("&", "")
            e.currentTarget.value = key;
        }
    } catch (ex) {
        console.error(ex);
    }
})

$("#test-video-btn").on('click', () => {
    let popup = new VideoPopup($("#yt-key-box")[0].value);
    popup.open();
})
$("#accept-tos-toggle").on('click', e => {
    let value = $(e.currentTarget).attr('value') == "true";
    $("#submit-btn")[0].disabled = value;
})
$("#submit-btn").on('click', async () => {
    let required = Array.from($("[required]"));
    let requirements = "";
    let missing = 0;
    for (let i = 0; i < required.length; i++) {
        let value = required[i].value == null ? $(required[i]).attr('value') : required[i].value;
        if (value == "" || value == null) {
            missing++;
            requirements += `<li>${$(`label[for="${required[i].id}"]`)[0].innerHTML}</li>`
        }
    }
    if (missing > 0) {
        await new ErrorPopup("Missing Requirement(s)", `<h4><ul>${requirements}</ul></h4>`).open();
    } else {
        let repoName = $("#github-repository-search-box")[0].value;
        let projectName = $("#project-name-box")[0].value;
        let tags = $("#tags-search-box")[0].value.split("; ");
        let keywords = $("#")

        let data = new FormData();

    }
});

function UploadGalleryImage(target) {

    let input = document.createElement('input');
    input.type = "file"
    input.accept = "image/*";
    $(input).on('change', l => {
        let file = l.currentTarget.files[0];
        let popup = new ImagePopup(target, file, 16 / 9, false, async base => {
            target.parentElement.parentElement.innerHTML += `
            <div class="list vertical">
                    <h4 class="file-upload-title">${file.name}</h4>
                    <div class="file-upload tmp-gallery-image" onclick="this.parentElement.remove()" style="background-image: url('data:image/png;base64,${base}')">
                        <div class="content">
                            <i class="fas fa-trash"></i>
                        </div>
                    </div>
                </div>`;
        });
        popup.open();
    })
    input.click();
}