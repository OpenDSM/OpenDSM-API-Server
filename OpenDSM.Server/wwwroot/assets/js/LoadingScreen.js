class LoadingScreen {
    #view
    constructor(title, message) {
        $("body")[0].style.overflow = "hidden"
        this.#view = document.createElement("div");
        this.#view.classList.add("fullscreen-view", "list", "vertical", "centered");

        let header = document.createElement('h1');
        header.innerHTML = title;

        let body = document.createElement("p");
        body.classList.add("paragraph-1");
        body.innerHTML = message;

        let throbber = document.createElement("span")
        throbber.classList.add("throbber");
        throbber.style.height = "100px";

        this.#view.append(header);
        this.#view.append(body);
        this.#view.append(throbber);
        $("body")[0].appendChild(this.#view);
        setTimeout(() => this.#view.classList.add("load"), 100)
    }

    unload() {
        this.#view.classList.add("unload")
        setTimeout(() => this.#view.remove(), 1000)
        $("body")[0].style.overflow = ""
    }
}