(() => {
    updateAbout($("#product-page-overview")[0].innerHTML)
    LoadPosterVideo();
    LoadVersions()
    LoadReviews()
    async function LoadReviews() {
        let section = $("section#reviews")[0]
        let url = `/product/${id}/element/Reviews`;
        let html = await $.get(url);
        section.innerHTML = html;
        InitElements()
    }
    async function LoadVersions() {
        let section = $("section#versions")[0]
        let url = `/product/${id}/element/Versions`;
        let html = await $.get(url);
        section.innerHTML = html;
    }
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

    document.cookie = `page_view=; path=/product/${id}`
}).call();