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

class ImagePopup extends Popup {
    file;
    aspectRatio;
    rounded;
    email;
    token;
    onupload;
    image;
    constructor(file, aspectRatio, rounded, onupload = () => { }) {
        super("image-cropper")
        this.file = file;
        this.aspectRatio = aspectRatio;
        this.rounded = rounded;
        this.onupload = onupload;
    }

    async open() {
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
        Array.from(document.cookie.split(';')).forEach(item => {
            let key = item.split("=")[0].trim()
            if (key == "auth_email") {
                this.email = item.replace(key + "=", "");
            }
            if (key == "auth_token") {
                this.token = item.replace(key + "=", "");
            }
        })
        setTimeout(() => {
            $(this.image).cropper({
                aspectRatio: this.aspectRatio,
                dragMode: "none",
                movable: false,
                zoomable: false,
            })
        }, 1000)

        $("#upload-cropped-image").on('click', this.onupload)
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