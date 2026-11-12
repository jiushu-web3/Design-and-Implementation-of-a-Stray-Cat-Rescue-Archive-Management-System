(function () {
    function show(root, text) {
        var box = root.querySelector("[data-form-message]");
        if (!box && root.parentNode) {
            box = root.parentNode.querySelector("[data-form-message]");
        }
        if (box) {
            box.textContent = text;
        }
    }

    function trimmed(value) {
        return (value || "").replace(/^\s+|\s+$/g, "");
    }

    function closestAttr(node, name) {
        while (node && node !== document) {
            if (node.getAttribute && node.getAttribute(name) === "true") {
                return node;
            }
            node = node.parentNode;
        }
        return null;
    }

    function validate(root) {
        var required = root.querySelectorAll("[data-required='true']");
        var i;
        for (i = 0; i < required.length; i++) {
            if (!trimmed(required[i].value)) {
                show(root, "请填写必填项");
                required[i].focus();
                return false;
            }
        }
        var phones = root.querySelectorAll("[data-phone='true']");
        var phonePattern = /^1[3-9]\d{9}$/;
        for (i = 0; i < phones.length; i++) {
            var phone = trimmed(phones[i].value);
            if (!phone) {
                continue;
            }
            if (!phonePattern.test(phone)) {
                show(root, "请填写正确的手机号");
                phones[i].focus();
                return false;
            }
        }
        var limited = root.querySelectorAll("[data-max]");
        for (i = 0; i < limited.length; i++) {
            var value = trimmed(limited[i].value);
            if (!value) {
                continue;
            }
            var max = parseInt(limited[i].getAttribute("data-max"), 10);
            var minAttr = limited[i].getAttribute("data-min");
            var min = minAttr ? parseInt(minAttr, 10) : 0;
            var rule = limited[i].getAttribute("data-rule");
            if (rule === "username" && !/^[A-Za-z0-9]{4,20}$/.test(value)) {
                show(root, "用户名须为 4 到 20 位字母或数字");
                limited[i].focus();
                return false;
            }
            if (rule === "captcha" && !/^[0-9A-Za-z]{4}$/.test(value)) {
                show(root, "验证码为 4 位数字或字母");
                limited[i].focus();
                return false;
            }
            if (value.length > max || (minAttr && value.length < min)) {
                show(root, rule === "password" ? "密码须为 6 到 20 位" : "内容超出允许的长度");
                limited[i].focus();
                return false;
            }
        }
        var compares = root.querySelectorAll("[data-compare]");
        for (i = 0; i < compares.length; i++) {
            var other = document.getElementById(compares[i].getAttribute("data-compare"));
            if (other && compares[i].value !== other.value) {
                show(root, "两次输入的密码不一致");
                compares[i].focus();
                return false;
            }
        }
        return true;
    }

    document.addEventListener("DOMContentLoaded", function () {
        var forms = document.querySelectorAll("form");
        var i;
        for (i = 0; i < forms.length; i++) {
            forms[i].addEventListener("submit", function (event) {
                var form = event.currentTarget;
                var submitter = event.submitter || document.activeElement;
                if (submitter && submitter.getAttribute && submitter.getAttribute("data-skip-validate") === "true") {
                    return;
                }
                var scope = null;
                if (form.getAttribute("data-validate") === "true") {
                    scope = form;
                } else if (submitter) {
                    scope = closestAttr(submitter, "data-validate");
                }
                if (!scope) {
                    return;
                }
                if (!validate(scope)) {
                    event.preventDefault();
                }
            });
        }
    });
})();

/* 导航高亮：按当前页面文件名给左侧菜单加 active，用于视觉定位 */
(function () {
    function currentPage() {
        var name = (location.pathname || "").split("/").pop();
        return (name || "Default.aspx").toLowerCase();
    }

    function markMenu() {
        var page = currentPage();
        var links = document.querySelectorAll(".menu a");
        for (var i = 0; i < links.length; i++) {
            var href = (links[i].getAttribute("href") || "").split("/").pop().toLowerCase();
            if (href === page) {
                links[i].className = links[i].className.replace(/\s*active\b/, "") + " active";
            }
        }
    }

    document.addEventListener("DOMContentLoaded", markMenu);
})();

/* 图片本地预览：选择照片后立即显示缩略图，避免上传后才发现选错 */
(function () {
    function preview(input) {
        var file = input.files && input.files[0];
        var box = input.parentNode.querySelector(".photo-preview");
        if (!file || !/^image\/(jpeg|png)$/.test(file.type)) {
            if (box) { box.parentNode.removeChild(box); }
            return;
        }
        if (!box) {
            box = document.createElement("img");
            box.className = "photo-preview";
            box.alt = "照片预览";
            box.style.cssText = "display:block;width:120px;height:120px;object-fit:cover;"
                + "margin-top:8px;border:1px solid #EFE4D8;border-radius:8px;background:#FBF7F1;";
            input.parentNode.appendChild(box);
        }
        if (box.src && box.src.indexOf("blob:") === 0) {
            URL.revokeObjectURL(box.src);
        }
        box.src = URL.createObjectURL(file);
    }

    document.addEventListener("DOMContentLoaded", function () {
        var inputs = document.querySelectorAll("input[type='file']");
        for (var i = 0; i < inputs.length; i++) {
            inputs[i].addEventListener("change", function () { preview(this); });
        }
    });
})();

/* 验证码：点击图片或“换一张”时只更换图片，不提交表单 */
(function () {
    function reload(img) {
        if (!img) {
            return;
        }
        var scope = img.getAttribute("data-captcha-scope") || "login";
        img.src = "Captcha.aspx?scope=" + encodeURIComponent(scope) + "&t=" + new Date().getTime();
    }

    document.addEventListener("DOMContentLoaded", function () {
        var links = document.querySelectorAll("[data-captcha-refresh]");
        var i;
        for (i = 0; i < links.length; i++) {
            links[i].addEventListener("click", function (event) {
                event.preventDefault();
                reload(document.getElementById(this.getAttribute("data-captcha-refresh")));
            });
        }
        var images = document.querySelectorAll("img[data-captcha-scope]");
        for (i = 0; i < images.length; i++) {
            images[i].addEventListener("click", function () { reload(this); });
        }
    });
})();
