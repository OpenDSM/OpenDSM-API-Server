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
            $("#search-video-btn")[0].title = "Tests the video key"
            $("#search-video-btn")[0].innerHTML = `<i class="fa-solid fa-vial"></i>`
        }
    } catch (ex) {
        console.error(ex);
    }
})
$("#yt-key-box").on('keyup', e => {
    let value = e.currentTarget.value;
    if (value == "") {
        $("#search-video-btn")[0].title = "Searches for Youtube Video"
        $("#search-video-btn")[0].innerHTML = `<i class="fa-solid fa-magnifying-glass"></i>`
    } else {
        $("#search-video-btn")[0].title = "Tests the video key"
        $("#search-video-btn")[0].innerHTML = `<i class="fa-solid fa-vial"></i>`
    }
})
$("#search-video-btn").on('click', () => {
    if ($("#yt-key-box")[0].value != "") {
        new VideoPopup($("#yt-key-box")[0].value).open();
    } else {
        new YoutubeSearchPopup().open();
    }
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
        let tags = $("#tags-search-box")[0].value;
        let keywords = $("#keywords-box")[0].value;
        let youtubeKey = $("#yt-key-box")[0].value;
        let subscription = $("#payment-type-box")[0].value == "Subscription";
        let price = $("#price-box")[0].value;
        let useGitReadme = $("#use-readme-toggle").attr("value") == "true";
        let about = $("#about-box")[0].value;
        let icon = $("#upload-icon").attr("value");
        let banner = $("#upload-banner").attr("value");
        let galleryImages = Array.prototype;

        Array.from($(".tmp-gallery-image")).forEach(item => {
            galleryImages.push(item.style.backgroundImage.replace("url(", "").replace(")", "").replaceAll("\"", "").replaceAll("'", ""))
        })
        let data = new FormData();
        data.append("name", projectName);
        data.append("gitRepoName", repoName);
        data.append("user_id", user_id);
        if (youtubeKey != "") {
            data.append("yt_key", youtubeKey);
        }
        data.append("subscription", subscription);
        data.append("price", price);
        data.append(`keywords`, keywords.toLowerCase());
        data.append(`tags`, tags);
        data.append("icon", icon);
        data.append("banner", banner);
        data.append("use_git_readme", useGitReadme);
        if (galleryImages.length > 0) {
            for (let i = 0; i < galleryImages.length; i++) {
                if (galleryImages[i] != null && galleryImages[i] != "")
                    data.append(`gallery[${i}]`, galleryImages[i]);
            }
        }
        let loadingScreen = new LoadingScreen("Creating Product", "This may take a moment...");
        let response = await fetch("/api/product", { method: "POST", body: data });
        if (response.ok) {
            let json = await response.json();
            window.location.href = `/product/${json.id}`
        }
        loadingScreen.unload();
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

function updateRepository(value) {
    $('#project-name-box')[0].value = value
    let keywords = value + ";";
    value.split(" ").forEach(i => {
        keywords += `${i};`
    })
    keywords += `${value.replaceAll(" ", "")};`
    $("#keywords-box")[0].value = keywords.toLowerCase();
}