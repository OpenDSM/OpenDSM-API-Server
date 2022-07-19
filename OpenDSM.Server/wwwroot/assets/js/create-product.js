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