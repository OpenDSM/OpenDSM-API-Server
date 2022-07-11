class Popup {
    name;
    constructor(name) {
        this.name = name;
    }

    async open() {
        if ($("#popup .content")[0].innerHTML.trim() != "") {
            await new Promise(r => setTimeout(r, 700))
        }
        let html = await $.get(`/assets/html/${this.name}.html`);
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