window.plantFormStorage = {
    save: function (data) {
        localStorage.setItem("plantFormDraft", JSON.stringify(data));
    },


load: function () {
    const data = localStorage.getItem("plantFormDraft");
    return data ? JSON.parse(data) : null;
},

clear: function () {
    localStorage.removeItem("plantFormDraft");
}


};
