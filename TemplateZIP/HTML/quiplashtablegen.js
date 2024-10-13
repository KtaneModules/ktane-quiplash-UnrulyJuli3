var fs = require("fs");
var qldata = JSON.parse(fs.readFileSync("D:/Steam/steamapps/common/The Jackbox Party Pack 7/games/Quiplash3/content/Quiplash3Round1Question.jet")).content.filter((q) => q.safetyQuips.length == 3);
//console.log(qldata);
var totalCount = 0;
var bgnum = 1;
var pgnum = 1;
var finalPages = [];
var hasReachedEnd = false;
var makePage = (ind) => {
    var pageCount = ind ? 19 : 12;
    var prompts = qldata.slice(totalCount, totalCount + pageCount);
    if (!prompts.length) return hasReachedEnd = true;
    totalCount += pageCount;
    //console.log(prompts);

    var rows = [];
    var cells = ["<th>ID</td>", "<th colspan=\"3\">Answers</td>"];
    rows.push(`<tr>\n\t\t\t\t${cells.join("\n\t\t\t\t")}\n\t\t\t</tr>`);
    prompts.forEach((p) => {
        var cells = [`<td>${p.id}</td>`];
        //var colspan = p.safetyQuips.length > 2 ? 3 : 2;
        //p.safetyQuips.forEach((q) => cells.push(`<td colspan="${colspan}">${q}</td>`));
        p.safetyQuips.forEach((q) => cells.push(`<td>${q}</td>`));
        rows.push(`<tr>\n\t\t\t\t${cells.join("\n\t\t\t\t")}\n\t\t\t</tr>`);
    });

    if (bgnum > 7) bgnum = 1;
    var page = `<div class="page page-bg-0${bgnum}">\n    <div class="page-header">\n        <span class="page-header-doc-title">Keep Talking and Nobody Explodes Mod</span>\n        <span class="page-header-section-title">Quiplash</span>\n    </div>\n    <div class="page-content">\n        <table>\n\t\t\t${rows.join("\n\t\t\t")}\n\t\t</table>\n    </div>\n    <div class="page-footer relative-footer">Page ${pgnum} of {{totalcount}}</div>\n</div>`.split(/\n/g).map((l) => "\t\t"+l).join("\n");
    finalPages.push(page);
    bgnum++;
    pgnum++;
};
var i = 0;
while (!hasReachedEnd) {
    makePage(i);
    i++;
}
finalPages = finalPages.map((p) => p.replace(/{{totalcount}}/g, finalPages.length));
//console.log(finalPages);
fs.writeFileSync("final.txt", finalPages.join("\n"), "utf-8");