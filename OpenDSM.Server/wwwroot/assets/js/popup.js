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
    author;
    id;
    version_id;
    constructor(author, id, version_id) {
        super("changelog")
        this.author = author
        this.id = id;
        this.version_id = version_id;
    }
    async open() {
        let element = await super.open();
        let response = await fetch(`/api/product/${this.author}/${this.id}/${this.version_id}`)
        if (response.ok) {
            let json = await response.json();
            $(element).find('h1.title')[0].innerText = json["version"]
            $(element).find('.description')[0].innerText = json["log"]
        } else {
            $(element).find('h1')[0].innerText = "Changelog Not Found"
            console.log($(element).find('.description')[0])
        }
    }

    close() {
        super.close();
    }
}

class DownloadPopup extends Popup {
    author;
    id;
    version_id;
    constructor(author, id, version_id) {
        super("download")
        this.author = author
        this.id = id;
        this.version_id = version_id;
    }
    async open() {
        let element = await super.open();
        let response = await fetch(`/api/product/${this.author}/${this.id}/${this.version_id}`)

        if (response.ok) {
            let json = await response.json();
            let url = json['url'];

            let blob = await fetch(url).then(r => r.blob());
            let link = document.createElement('a');
            link.href = URL.createObjectURL(blob);
            link.download = `${this.author}-${this.id}-${this.version_id}.zip`
            link.click();
        } else {
            $(element).find('h1')[0].innerText = "Download Not Found"
            $(element).find('p')[0].innerText = "Sorry, sometimes life give's us problems without solutions..."
        }
    }

    close() {
        super.close();
    }
}

class ProfileImagePopup extends Popup {
    file;
    constructor(file) {
        super("image-cropper")
        this.file = file;
    }

    async open() {
        await super.open();
        $(".image-container")[0].classList.add('rounded');
        let reader = new FileReader();
        let image = $("#cropper-image-canvas")[0];
        reader.onload = e => {
            $(image).attr("src", e.target.result)
        }
        reader.readAsDataURL(this.file)
        setTimeout(() => {
            $(image).cropper({
                aspectRatio: 1 / 1,
                dragMode: "none",
                movable: false,
                zoomable: false,
            })
        }, 1000)
        $("#upload-cropped-image").on('click', async () => {
            let imageUrl = $(image).data('cropper').getCroppedCanvas().toDataURL().split('base64,')[1];
            let data = new FormData();
            data.append("base64", imageUrl);
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
            data.append("email", email);
            data.append("token", token);
            await fetch('/api/auth/image/profile', { method: "POST", body: data })

            window.location.reload();
        })
    }
}

class ProfileBannerPopup extends Popup {
    file;
    constructor(file) {
        super("image-cropper")
        this.file = file;
    }

    async open() {
        await super.open();
        let reader = new FileReader();
        let image = $("#cropper-image-canvas")[0];
        reader.onload = e => {
            $(image).attr("src", e.target.result)
        }
        reader.readAsDataURL(this.file)
        setTimeout(() => {
            $(image).cropper({
                aspectRatio: 16 / 3.6667,
                dragMode: "none",
                movable: false,
                zoomable: false,
            })
        }, 1000)
        $("#upload-cropped-image").on('click', async () => {
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

            let imageUrl = $(image).data('cropper').getCroppedCanvas().toDataURL().split('base64,')[1];
            let data = new FormData();
            data.append("base64", imageUrl);

            data.append("email", email);
            data.append("token", token);
            await fetch('/api/auth/image/banner', { method: "POST", body: data })
            window.location.reload();
        })
    }
}