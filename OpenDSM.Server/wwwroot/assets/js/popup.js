class Popup {
    name;
    constructor(name) {
        this.name = name;
    }

    async open() {
        if ($("#popup .content")[0].innerHTML.trim() != "") {
            await new Promise(r => setTimeout(r, 700))
        }
        let html = await $.get(`/popup/${this.name}`);
        $("#popup .content")[0].innerHTML = html;
        $("#popup")[0].classList.add('active')
        $("body")[0].style.overflow = "hidden"
        $("#popup-close-btn, #popup-bg").on('click', () => {
            this.close()
        })

        InitElements();
        return $("#popup .content")[0]
    }
    close() {
        $("#popup")[0].classList.remove('active')
        $("body")[0].classList.remove('popup-active')
        setTimeout(() => {
            $("#popup .content")[0].innerHTML = "";
        }, 500)

        $("body")[0].style.overflow = ""
    }
}

class CenteredPopup extends Popup {
    constructor(name) {
        super(name);
    }
    async open() {
        let element = await super.open();
        $("#popup-content .content")[0].style.display = "flex"
        $("#popup-content .content")[0].style.justifyContent = "center"
        $("#popup-content .content")[0].style.alignItems = "center"
        return element;
    }
    async close() {
        $("#popup-content .content")[0].style.display = ""
        $("#popup-content .content")[0].style.justifyContent = ""
        $("#popup-content .content")[0].style.alignItems = ""
        await super.close();
    }
}

class ErrorPopup extends CenteredPopup {
    message;
    title;
    constructor(title, message) {
        super('error')
        this.title = title;
        this.message = message;
    }
    async open() {
        await super.open();
        $("#error-title")[0].innerText = this.title;
        $("#error-body")[0].innerHTML = this.message;
    }
}

class VideoPopup extends Popup {
    url;
    player;
    constructor(url) {
        super("video");
        this.url = url;
    }

    async open() {
        this.player = new DSMPlayer($(await super.open()).find('dsmplayer')[0].id, this.url, false);
        this.player.play();
    }

    close() {
        this.player.pause();
        super.close();
    }
}

class GalleryPopup extends Popup {
    img;
    constructor(element) {
        super("empty");
        this.img = element.style.backgroundImage
    }

    async open() {
        await super.open();
        $("#popup .content")[0].style.backgroundImage = this.img;
    }

    close() {
        super.close();
        $("#popup .content")[0].style.backgroundImage = "";
    }
}
class PurchasePopup extends Popup {
    author;
    id;
    isSubscription;
    constructor(author, id, isSubscription) {
        super(isSubscription ? "subscription" : "purchase");
        this.author = author;
        this.id = id;
        this.isSubscription = isSubscription;
    }

    async open() {
        await super.open();
    }

    close() {
        super.close();
    }
}

class ChangelogPopup extends Popup {
    changelog;
    version;
    constructor(version, changelog) {
        super("changelog")
        this.changelog = changelog;
        this.version = version;
    }
    async open() {
        let element = await super.open();
        $(element).find('h1')[0].innerText = this.version;
        let converter = new showdown.Converter()
        $(element).find('.description')[0].innerHTML = converter.makeHtml(this.changelog)
    }

    close() {
        super.close();
    }
}

class DownloadPopup extends CenteredPopup {
    product_name;
    id;
    version_id;
    platform;
    version_name;
    constructor(product_name, id, version_id, platform, version_name) {
        super("download")
        this.product_name = product_name;
        this.id = id;
        this.version_id = version_id;
        this.platform = platform;
        this.version_name = version_name;
    }
    async open() {
        let element = await super.open();
        let link = $(element).find("#download-link")[0];
        link.href = `/api/product/${this.id}/version/${this.version_id}?platform=${this.platform}`;
        link.download = `${this.product_name}-${this.platform}-${this.version_name}.zip`
        link.click();
    }

    close() {
        super.close();
    }
}

class ImagePopup extends Popup {
    button;
    file;
    aspectRatio;
    rounded;
    email;
    token;
    onupload;
    image;
    constructor(button, file, aspectRatio, rounded, onupload = base => { }) {
        super("image-cropper")
        this.button = button;
        this.file = file;
        this.aspectRatio = aspectRatio;
        this.rounded = rounded;
        this.onupload = onupload;
    }

    async open() {
        if (this.file.size > (Math.pow(2, 20) * 4)) {
            alert("File size cannot exceed 4MB")
            return;
        }
        await super.open();
        if (this.rounded) {
            $(".image-container")[0].classList.add('rounded');
        }
        let reader = new FileReader();
        this.image = $("#cropper-image-canvas")[0];
        reader.onload = e => {
            $(this.image).attr("src", e.target.result)
        }
        reader.readAsDataURL(this.file)
        setTimeout(() => {
            $(this.image).cropper({
                aspectRatio: this.aspectRatio,
                dragMode: "none",
                movable: false,
                zoomable: false,
                responsive: true,
                restore: false,
            })
        }, 1000)

        $("#upload-cropped-image").on('click', e => {
            e.currentTarget.disabled = true;
            let base = $(this.image).data('cropper').getCroppedCanvas().toDataURL();
            (this.onupload).call(null, base.split('base64,')[1]);
            this.button.style.backgroundImage = `url('${base}')`
            $(this.button).attr("value", base)
            this.close();
        })
    }
}

class ActivateDeveloperAccountPopup extends Popup {
    constructor() {
        super("devaccount")
    }

    async open() {
        await super.open();

        $("#dev-account-activation-button").on('click', async e => {
            let git_username = $("#git-username-box")[0].value;
            let git_token = $("#git-token-box")[0].value;
            if (git_username == "" || git_token == "") {
                alert('Git Username and Token MUST be filled out!');
            } else {
                let email, token;
                Array.from(document.cookie.split(';')).forEach(item => {
                    let key = item.split("=")[0].trim()
                    if (key == "auth_email") {
                        email = item.replace(key + "=", "");
                    }
                    if (key == "auth_token") {
                        token = item.replace(key + "=", "");
                    }
                })

                let data = new FormData();

                data.append("email", email);
                data.append("token", token);
                data.append("git_username", git_username)
                data.append("git_token", git_token)

                let response = await fetch("/api/auth/activate-dev-account", { method: "POST", body: data })

                if (response.ok) {
                    window.location.reload();
                } else {
                    alert("Unable to activate account! Please check your credentials!");
                }
            }
        })
    }
}

class YoutubeSearchPopup extends CenteredPopup {
    constructor() {
        super("youtube")
    }

    async open() {
        await super.open();
        $("#channel-id").on('keyup', e => {
            if (e.key == "Enter") {
                $("#load-yt-list")[0].click();
            }
        })
        $("#load-yt-list").on("click", async () => {
            let id = $("#channel-id")[0].value;
            let loading = new LoadingScreen("Getting Youtube Videos", "This may take a moment...");
            let response = await fetch(`/api/yt/channel/${id}`)
            loading.unload();
            $("body")[0].style.overflow = "hidden"
            if (response.ok) {
                $("#yt-channel-search-box")[0].style.display = "none"
                let videos = $("#yt-videos")[0]
                videos.innerHTML = "";
                videos.style.display = "";
                let json = await response.json();
                for (let i = 0; i < json.length; i++) {
                    let video = json[i];

                    let vid = document.createElement('div')
                    vid.classList.add("yt-vid");

                    let link = document.createElement("div");
                    link.classList.add("list", "vertical");
                    $(link).on('click', () => {
                        $('#yt-key-box')[0].value = video.url.split('watch?v=').pop().split("&")[0]
                        $("#search-video-btn")[0].title = "Tests the video key"
                        $("#search-video-btn")[0].innerHTML = `<i class="fa-solid fa-vial"></i>`
                        this.close();
                    })

                    let img = document.createElement('img');
                    img.src = video.thumbnails[0].url;

                    let title = document.createElement('p');
                    title.classList.add("paragraph-1");
                    title.innerText = video.title;

                    let openBtn = document.createElement('a');
                    openBtn.classList.add('btn')
                    openBtn.href = video.url;
                    openBtn.target = "_blank";
                    openBtn.innerText = "Open"

                    link.appendChild(img)
                    link.appendChild(title)
                    vid.appendChild(link);
                    vid.appendChild(openBtn);

                    videos.appendChild(vid);
                }
            } else {
                new ErrorPopup("Channel Not Found", `Unable to find channel with id of '${id}'<br />Please double check your id and try again...`).open();
            }
        })
    }
}

class SearchFilterPopup extends Popup {
    constructor() {
        super("search-filter")
    }

    async open() {
        super.open();
        setTimeout(() => {
            $(".search-category").on('click', e => {
                let parts = window.location.search.split("?category=").pop().split("&");
                let category = parts[0]
                let tags = "";
                if (parts.length > 1) {
                    tags = parts.pop().split("tags=").pop();
                    if (e.currentTarget.classList.contains("active")) {
                        // Remove Tag
                        tags = decodeURI(tags).replace(`${$(e.currentTarget).attr("filter")};`, "").replace(`${$(e.currentTarget).attr("title")}`, "")
                    } else {
                        // Add Tag
                        tags += `${$(e.currentTarget).attr("filter")};`
                    }
                } else {
                    tags += `${$(e.currentTarget).attr("filter")};`
                }
                window.history.pushState("", "", `search?category=${category}&tags=${tags}`)
                e.currentTarget.classList.toggle("active");
            })
        }, 500)
    }
}

class CreateVersionPopup extends Popup {
    product_id;
    constructor(product_id) {
        super('create-version')
        this.product_id = product_id;
    }

    async open() {
        let element = await super.open();
        $(element).find(".file-upload").on('click', e => {
            if (e.currentTarget.classList.contains("active")) {
                $(`input[type="file"]#${e.currentTarget.id}-input`)[0].remove()
                $(e.currentTarget).find("i")[0].classList.remove("fa-trash")
                $(e.currentTarget).find("i")[0].classList.add("fa-plus")

                e.currentTarget.classList.remove('active');
            } else {
                let input = document.createElement("input");
                input.type = "file";
                input.accept = "application/zip"
                input.id = `${e.currentTarget.id}-input`;
                $(input).on('change', el => {
                    $(e.currentTarget).find("i")[0].classList.remove("fa-plus")
                    $(e.currentTarget).find("i")[0].classList.add("fa-trash")
                    $(e.currentTarget)[0].classList.add('active');
                    element.appendChild(input);
                })
                input.click();
                input.style.display = "none";
            }
        })
        let check = setInterval(() => {
            let version = $("input#version-name")[0].value;
            let releaseType = $("input#release-type")[0].value;
            let changelog = $("textarea#changelog-box")[0].value;
            let inputs = Array.from($(element).find('input[type="file"]'))

            if (version != "" && releaseType != "" && changelog != "", inputs.length != 0) {
                $(element).find('button.btn#finish-button')[0].disabled = false;
            } else {
                $(element).find('button.btn#finish-button')[0].disabled = true;
            }
        }, 500);
        $(element).find('button.btn#finish-button').on('click', async () => {
            clearInterval(check);
            window.onbeforeunload = function () {
                return "Are you sure? Unsaved changes maybe dismissed or corrupted";
            }
            let version = $("input#version-name")[0].value;
            let releaseType = $("input#release-type")[0].value;
            let changelog = $("textarea#changelog-box")[0].value;
            let inputs = Array.from($(element).find('input[type="file"]'))

            if (version != "" && releaseType != "" && changelog != "", inputs.length != 0) {
                let info = $("#info-section")[0]
                let uploads = $("#upload-section")[0]
                let uploadTasks = $("#upload-tasks")[0]
                info.style.display = "none";
                uploads.style.display = "";

                let name = "Creating Git Release";
                let header = document.createElement("h4");
                header.classList.add("upload-task");
                header.id = `${name}-upload-task`;

                let icon = document.createElement("i");
                icon.classList.add("throbber");

                header.appendChild(icon);
                header.append(name);
                uploadTasks.appendChild(header);

                let data = new FormData();
                data.append("id", this.product_id);
                data.append("name", version)
                data.append("type", releaseType)
                data.append("changelog", changelog);

                let response = await fetch(`/api/product/version`, { method: "POST", body: data });
                let itemsUploaded = 0;
                if (response.ok) {
                    let json = await response.json();
                    let version_id = json["id"];

                    icon.classList.remove('throbber');
                    icon.classList.add('fa', 'fa-check');

                    inputs.forEach(async i => {
                        let name = i.id.replace("-upload-input", "");
                        let header = document.createElement("h4");
                        header.classList.add("upload-task");
                        header.id = `${name}-upload-task`;

                        let icon = document.createElement("i");
                        icon.classList.add("throbber");

                        header.appendChild(icon);
                        header.append(`Uploading ${name}`);

                        uploadTasks.appendChild(header);

                        try {
                            let data = new FormData()
                            data.append("file", i.files[0]);
                            let response = await fetch(`/api/product/${this.product_id}/version/${version_id}/asset?platform=${name}`, { method: "POST", body: data })
                            itemsUploaded++;
                            icon.classList.remove('throbber');
                            if (response.ok) {
                                icon.classList.add('fa', 'fa-check');
                            } else {
                                icon.classList.add('fa-solid', 'fa-x');
                            }
                        } catch (error) {
                            console.log(response)
                            console.log("Error")
                            console.error(error)
                        }
                    })
                } else {
                    icon.classList.remove('throbber');
                    icon.classList.add('fa-solid', 'fa-x');
                }

                check = setInterval(async () => {
                    if (itemsUploaded == inputs.length) {
                        clearInterval(check);
                        name = "Saving";
                        header = document.createElement("h4");
                        header.classList.add("upload-task");
                        header.id = `${name}-upload-task`;

                        icon = document.createElement("i");
                        icon.classList.add("throbber");

                        header.appendChild(icon);
                        header.append(name);
                        uploadTasks.appendChild(header);
                        await fetch(`/api/product/${id}/version/check`, { method: "POST" })
                        icon.classList.remove('throbber');
                        icon.classList.add('fa', 'fa-check');

                        $(element).find('button.btn#done-button').on('click', () => {
                            window.location.reload()
                        })
                        window.onbeforeunload = null;
                        $(element).find('button.btn#done-button')[0].disabled = false;
                    }
                }, 500)
            }
        });
    }
}

class EditVersionPopup extends Popup {
    ID;
    ProductID;
    Name;
    Changelog;
    Release;
    constructor(ID, ProductID, Name, Changelog, Release) {
        super("edit-version")
        this.ID = ID;
        this.ProductID = ProductID;
        this.Name = Name;
        this.Changelog = Changelog;
        this.Release = Release;
    }

    async open() {
        let element = await super.open();
        $(element).find("#version-name")[0].value = this.Name;
        $(element).find("#changelog-box")[0].value = this.Changelog;
        $(element).find("#release-type")[0].value = this.Release;
        $(element).find("#finish-button").on('click', async () => {
            let loading = new LoadingScreen("Updating Version", "Don't go anywhere!")

            this.close();

            let name = $(element).find("#version-name")[0].value
            let changelog = $(element).find("#changelog-box")[0].value
            let release = $(element).find("#release-type")[0].value

            let data = new FormData()
            data.append('name', name)
            data.append('changelog', changelog)
            data.append('type', release)

            let response = await fetch(`/api/product/${this.ProductID}/version/${this.ID}`, { method: "PATCH", body: data })
            console.log(response)
            if (!response.ok) {
                let json = await response.json();
                alert(json["message"]);
            }

            loading.unload();
            window.location.reload();
        })
    }
}

class DeleteVersionPopup extends CenteredPopup {
    ID;
    ProductID;

    constructor(ID, ProductID) {
        super("delete-version")
        this.ID = ID;
        this.ProductID = ProductID;
    }

    async open() {
        let element = await super.open();
        $("#delete-button").on('click', async () => {
            let loading = new LoadingScreen("Deleting Version", "Don't go anywhere!")

            this.close();
            let response = await fetch(`/api/product/${this.ProductID}/version/${this.ID}`, {method: "DELETE"})

            if (!response.ok) {
                let json = await response.json();
                alert(json["message"]);
            } else {
                $(`.version-item#${this.ID}`)[0].remove();
            }

            loading.unload();
        })
    }
}