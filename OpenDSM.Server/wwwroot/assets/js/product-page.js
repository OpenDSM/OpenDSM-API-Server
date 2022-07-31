updateAbout($("#product-page-overview")[0].innerHTML)
LoadPosterVideo();
async function updateAbout(value) {
    let converter = new showdown.Converter()
    $("#product-page-overview")[0].innerHTML = converter.makeHtml(value);

    $("#product-page-overview")[0].style.display = "";
}
async function LoadPosterVideo() {
    if ($("video.poster").length != 0) {
        $("video.poster")[0].src = $("video.poster")[0].dataset.src;
    }
}