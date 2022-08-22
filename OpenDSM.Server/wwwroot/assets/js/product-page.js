(() => {
    updateAbout($("#product-page-overview")[0].innerHTML)
    LoadPosterVideo();
    LoadVersions()
    LoadReviews()
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


async function LoadReviews(rating = -1) {
    let section = $("section#reviews")[0]
    let url = `/product/${id}/element/Reviews?filter=${rating}`;
    let html = await $.get(url);
    section.innerHTML = html;
    InitElements()

    let converter = new showdown.Converter()
    Array.from($(".testimonial-description")).forEach(i => {
        i.innerHTML = converter.makeHtml(i.innerText);
    })
    if ($(".reviews .bars > .ratings-bar.active").length != 0) {
        let bar = $(".reviews .bars > .ratings-bar.active")[0];
        let bars = Array.from(bar.parentElement.querySelectorAll(".ratings-bar"));

        bars.forEach(b => {
            if (!b.classList.contains('active')) {
                b.style.filter = 'brightness(0.1)'
            }
        })

    } else {
        let bar = $(".reviews .bars > .ratings-bar")[0];
        let bars = Array.from(bar.parentElement.querySelectorAll(".ratings-bar"));
        bars.forEach(b => {
            b.style.filter = ''
        })
    }
    Array.from($(".reviews .bars > .ratings-bar")).forEach(barIndex => {
        $(barIndex).on('mouseenter', e => {
            if ($(".reviews .bars > .ratings-bar.active").length != 0) {
                let bar = $(".reviews .bars > .ratings-bar.active")[0];
                let bars = Array.from(bar.parentElement.querySelectorAll(".ratings-bar"));

                bars.forEach(b => {
                    if (!b.classList.contains('active')) {
                        b.style.filter = 'brightness(0.1)'
                    }
                })

            }
            let current = e.currentTarget;
            let bars = Array.from(current.parentElement.querySelectorAll(".ratings-bar"));


            for (let i = 0; i < bars.length; i++) {
                if (bars[i] == current) {
                    bars[i].style.filter = ''
                } else {
                    bars[i].style.filter = 'brightness(0.1)'
                }
            }
        })
        $(barIndex).on('click', e => {
            let current = e.currentTarget;
            if (current.classList.contains('active')) {
                LoadReviews();
            } else {
                LoadReviews(Number.parseInt(current.querySelector(".paragraph-1").innerText));
            }
        })
        $(barIndex).on('mouseleave', e => {
            let current = e.currentTarget;
            let bars = Array.from(current.parentElement.querySelectorAll(".ratings-bar"));
            for (let i = 0; i < bars.length; i++) {
                bars[i].style.filter = ''
            }

            if ($(".reviews .bars > .ratings-bar.active").length != 0) {
                let bar = $(".reviews .bars > .ratings-bar.active")[0];
                let bars = Array.from(bar.parentElement.querySelectorAll(".ratings-bar"));

                bars.forEach(b => {
                    if (!b.classList.contains('active')) {
                        b.style.filter = 'brightness(0.1)'
                    }
                })

            }
        })
    })

    $("button#leave-review-button").on('click', async () => {
        let form = $("#leave-review-form")[0];
        let subjectInput = $(form).find('#review-subject-box')[0];
        let ratingInput = $(form).find('.stars-input')[0];
        let bodyInput = $(form).find('#review-body-box')[0];

        let ok = true;
        if (subjectInput.value == "") {
            ok = false;
            subjectInput.parentElement.classList.add('shake');
            setTimeout(() => subjectInput.parentElement.classList.remove('shake'), 1000)
        }
        if (bodyInput.value == "") {
            ok = false;
            bodyInput.parentElement.classList.add('shake');
            setTimeout(() => bodyInput.parentElement.classList.remove('shake'), 1000)
        }

        if (Number.parseInt($(ratingInput).attr("value")) == 0) {
            ok = false;
            KickStars(Array.from(ratingInput.querySelectorAll('.star-input')))
        }

        if (ok) {
            let data = new FormData();
            data.append("rating", Number.parseInt($(ratingInput).attr("value")));
            data.append("body", bodyInput.value);
            data.append("summery", subjectInput.value);
            let response = await fetch(`/api/product/${id}/reviews`, { method: 'POST', body: data })
            if (!response.ok) {
                try {
                    let json = await response.json();
                    new ErrorPopup("Unable to submit Review", `ERROR: ${json.message}`).open();
                } catch {
                    new ErrorPopup("Unable to submit Review", `ERROR: Internal Server Error`).open();
                }
            } else {
                LoadReviews()
            }
        }


    })
}
async function LoadVersions() {
    let section = $("section#versions")[0]
    let url = `/product/${id}/element/Versions`;
    let html = await $.get(url);
    section.innerHTML = html;
    //await new Promise(r => setTimeout(r, 700))
}