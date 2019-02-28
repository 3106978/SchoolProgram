﻿var login, selectedClassName, password, userID, selectedDate, selectedClassID, countOfPresencePupils, teacherID, textArea;
var user = {};
var newUser = {};
var classes = [];
var lessons = [];
var pupilsToAttendance = [];
var pupilsFromMyClass = [];
var backButton = $('<input type="button" id="backButton"  value="back" onclick="window.location.reload();"/>');
var btnClose = $('<button class="btnClosePage" type="button" onclick="window.open(\'\', \'_self\', \'\'); window.close();">Close</button>');
var schedule = [];
class PupilsAttendanceWithComment {
    constructor(list, comment) {
        this.attendanceList = list;
        this.teachersComment = comment;
    }
}


$(function () {
    $("#txtLogin").focus();
});

function enterkeyPressed(event, btnName) {
    if (event.keyCode === 13) {
        if (btnName === "btnLogin")
            btnLogin();
        else if (btnName === "btnSignup")
            signUp();
    }

}
function btnLogin() {
    login = $("#txtLogin").val();
    password = $("#txtPassword").val();
    if (login.length === 0 || password.length === 0) {
        $("#spanLoginResult").html("Must type user and password!");
        return;
    }
    $("#spanLoginResult").html("Please wait...");
    $("#btnLogin").attr("disabled", true);

    $.get("api/LogAndPas?login=" + login + "&password=" + password, function (data, status) {
        $("#btnLogin").attr("disabled", false);
        if (status === "success") {
            if (data) {
                $(".divLogin").hide();
                user = data;
                userID = data.userID;

                if (user.isNewUser) {

                    $("#divSignup").show();
                    $("#txtEmail").focus();
                    $("#btnSignUp").attr("disabled", false);
                    if (user.isATeacher) {
                        getTeacherID();

                    }
                }
                else if (user.isAdmin)
                    AdminPage();

                else if (user.isATeacher) {
                    getTeacherID();
                    teacherPage();
                }

                else if (!user.isNewUser)
                    userPage();
            }
            else {
                $("#spanLoginResult").html("The data is not valid. Please try again");
                return;
            }
        }
        else {
            $("#spanLoginResult").html("Error!");
            return;
        }
    });
}



function signUp() {
    var eMail = $("#txtEmail").val();
    var newpassword = $("#txtNewPassword").val();
    $("#divSignup").append(backButton);
    if (eMail.length < 6 || newpassword.length < 6 || newpassword > 8) {
        $("#spanLoginResult").text("Entered data is not valid! Please, try again.");
        return;
    }
    newUser = {
        UserID: user.userID,
        EMail: eMail,
        LastLogin: new Date(),
        NewPassword: newpassword
    };
    $("#btnSignUp").attr("disabled", true);

    $.ajax({
        type: "POST",
        url: "api/Users",
        dataType: "json",
        contentType: "application/json",
        data: JSON.stringify(newUser),
        success: function (msg) {
            if (msg === true) {
                $("#spanLoginResult").text("The SignUp success!");
                $("#btnSignUp").attr("disabled", false);
                if (user.isATeacher) {
                    $("#divSignup").hide();
                    teacherPage();
                }
                else if (!user.isATeacher && !user.isAdmin) {
                    $("#divSignup").hide();
                    userPage();
                }
            }
            else {
                $("#spanLoginResult").text("The SignUp is not successed!");
                return;
            }
        },
        failure: function (msg) {
            $("#spanLoginResult").text("error from server side. The response is " + msg);
            return;
        }
    });


}

function userPage() {
    $("#userPage").show();
    $("#h1Hello").text("Hello ");
}

function teacherPage() {
    $("#divTeacherPage").show();
    $("#divTeacherPage").append(backButton);
}

//the method is executed after the Attendance.html page is loaded and is intended to query
//from the database a list of all classes existing in the school, display it 
//and establish dates available in the calendar for selection
function renderClassesNameSelector() {
    var today = getDateofTodayInFormat();
    $("#calendar").attr("max", today);
    $("#calendar").attr("value", today);
    $(".spanResult").html("");
    $(".divSelectContainer").append(btnClose);
    $.get("api/Teacher/GetClasses", function (data, status) {
        $("#btnShowAttendanceList").attr("disabled", false);
        if (status === "success") {
            classes = data;
            if (classes && classes.length > 0) {
                for (var i = 0; i < classes.length; i++) {
                    $("#selectClass").append('<option value=' + classes[i].className + '>' + classes[i].className + '</option>');
                }
            }
            else {
                $(".spanResult").html("Oops! Classes is null...");
                return;
            }
        }
        else {
            $(".spanResult").html("Oops! server is not response...");
            return;
        }
    });

}
//the method formats the date chosen by the client to the date format for the database
// to 'yyyy - mm - dd'
function getDateofTodayInFormat() {
    var today = new Date();
    var dd = today.getDate();
    var mm = today.getMonth() + 1;
    var yyyy = today.getFullYear();
    if (dd < 10) {
        dd = '0' + dd;
    }
    if (mm < 10) {
        mm = '0' + mm;
    }
    today = yyyy + '-' + mm + '-' + dd;
    return today;
}

function renderLessonsListToSelection() {
    $(".divText").hide();
    $(".spanResult").text('');
    selectedClassName = $("#selectClass :selected").val();
    if (selectedClassName === "Choose here") {
        $(".spanResult").text("Must choose a class!").css({ 'font-size': '30px', 'font-color': 'white' });
        return;
    }

    $(".divSelectContainer").hide();

    for (let i = 0; i < classes.length; i++) {
        if (selectedClassName === classes[i].className) {
            selectedClassId = classes[i].classID;
            break;
        }
    }

    selectedDate = $("#calendar").val();
    $.get("api/Teacher/GetLessons?date=" + selectedDate + "&classID=" + selectedClassId, function (data, status) {
        if (status === "success") {

            $(".divLessonsContainer").show();
            lessons = data;
            if (lessons && lessons.length > 0) {
                let table = $('<table></table>').addClass('lessonsTable');
                for (let i = 0; i < lessons.length; i++) {

                    let cell = '<td>' + lessons[i].numberOfLesson + '</td><td><a onclick="renderAttendanceTable(' + i +
                        ')" class="link">' + lessons[i].lessonName + '</a></td>';
                    let row = $('<tr></tr>').append(cell);
                    table.append(row);
                }
                $(".divLessonsContainer").append(table);
            }
            else {

                let h1 = $('<h1 align="center" id="resultH1"></h1>').text('There are no lessons on this day by Schedule');
                $(".divLessonsContainer").append(h1);
            }
            backButton.css('margin-left', '45%');
            $(".divLessonsContainer").append(backButton);
        }
        else {
            $(".spanResult").html("Oops! Something wrong in server side...");
        }
    });
}

function renderAttendanceTable(i) {

    $(".divText").show();
    $(".divLessonsContainer").hide();
    $(".spanResult").text("");
    $.get("api/Teacher/GetPupilsListToAttendance?date=" + selectedDate + "&classId=" + selectedClassId +
        "&lessonId=" + lessons[i].lessonID + "&numberOfLesson=" + lessons[i].numberOfLesson, function (data, status) {
            if (status === "success") {
                $(".divAttendanceContainer").show();
                pupilsToAttendance = data.attendanceList;
                let commentOfLesson = data.teachersComment;
                if (commentOfLesson === null) {
                    commentOfLesson = "";
                }
                if (data && pupilsToAttendance.length > 0) {
                    renderTable();
                    textArea = $("<textarea id='textarea'></textarea>").addClass("textarea");
                    let hComment = $("<h2>Comment to lesson</h2>");
                    let divText = $(".divText").append(hComment);

                    if (pupilsToAttendance[0].presence === null) {
                        let btn = "<button id='btnSendAttendanceToServer' onclick='sendAttendanceToServer()'>Save</button>";
                        divText.append(textArea);
                        $(".divButtons").append(btn);
                    }
                    else {
                        let div = $('<div id="divComment" style="padding:10px;"><p>' + commentOfLesson + '</p></div>');
                        $(".divText").append(div);
                    }
                    $(".divButtons").append(backButton);
                }

                else {
                    $(".spanResult").text("Ooops! Response data is not exists!");
                    return;
                }
            }

            else
                $(".spanResult").text("Ooops! Something wrong in server side!");
        });
}
function getTeacherID() {
    $(".spanResult").text("");
    $.get("api/Teacher/GetTeacherID?userID=" + userID, function (data, status) {
        if (status === "success") {
            if (data) {
                teacherID = data;
                localStorage['teacherID'] = teacherID;
                return teacherID;
            }
            else {
                $(".spanResult").text("Ooops! the data is null!");
                return;
            }
        }
        else {
            $(".spanResult").text("Ooops! Something wrong in server side!");
            return;
        }
    });
}
function sendAttendanceToServer() {

    $("#btnSendAttendanceToServer").attr("disabled", true);
    $("tr.trAttendance").each(function (i) {
        pupilsToAttendance[i].presence = $(this).find('input:checkbox').is(':checked');
        pupilsToAttendance[i].pupil.pupilID = Number($(this).find("input:checkbox").val());
        pupilsToAttendance[i].teacher.teacherID = localStorage['teacherID'];


        var commentFromInput = $(this).find('input:text').val();
        if (commentFromInput !== undefined)
            pupilsToAttendance[i].comment = commentFromInput;
    });
    var attendanceFromClientWithComment = new PupilsAttendanceWithComment(pupilsToAttendance, $("#textarea").val());
    $.ajax({
        type: "POST",
        url: "api/Teacher",
        dataType: "json",
        contentType: "application/json",
        data: JSON.stringify(attendanceFromClientWithComment),
        success: function (result) {
            if (result !== null) {
                $("#btnSendAttendanceToServer").hide();
                $(".attendanceTable").hide();
                for (var i = 0; i < pupilsToAttendance.length; i++) {
                    pupilsToAttendance[i].teacher.name = result.name;
                    pupilsToAttendance[i].teacher.surname = result.surname;

                }
                renderTable();
                textArea.hide();
                $(".divText").append(textArea.val());
            }
            else {
                $(".spanResult").text("Ooops! Something wrong!");
            }

        },
        failure: function (msg) {
            alert("error" + msg);
        }
    });
}



function renderTable() {
    var table = $("<table></table>").addClass("attendanceTable");
    var imgV = "<img src='images/vImg.jpg' alt='true'>";
    var imgMinus = "<img src='images/minus.png' alt='false'>";
    countOfPresencePupils = 0;
    let header1 = $("<tr></tr>").html("<th></th><th></th><th>Class: " + selectedClassName +
        "</th><th>" + selectedDate + "</th><th></th>");
    table.append(header1);
    let header2 = $("<tr></tr>").html("<th></th><th>Pupils</th><th>Presence</th><th>Teacher</th><th>Comment</th>");
    table.append(header2);
    for (var i = 0; i < pupilsToAttendance.length; i++) {
        var cells = "<td>" + (i + 1) + "</td><td>";
        cells += pupilsToAttendance[i].pupil.name + " " + pupilsToAttendance[i].pupil.surname + "</td>";
        if (pupilsToAttendance[i].presence === null) {
            cells += "<td><input class='checkbox' type='checkbox' value='" + pupilsToAttendance[i].pupil.pupilID + "'/></td>";
        }
        else if (pupilsToAttendance[i].presence) {
            cells += "<td>" + imgV + "</td>";
            countOfPresencePupils++;
        }
        else if (!pupilsToAttendance[i].presence) {
            cells += "<td>" + imgMinus + "</td>";
        }
        cells += "<td>" + pupilsToAttendance[i].teacher.name + " " +
            pupilsToAttendance[i].teacher.surname + "</td>";
        if (pupilsToAttendance[i].comment === null)
            cells += "<td class='tdComment'><input type='text' id='inputComment'/></td>";
        else if (pupilsToAttendance[i].comment !== null) { cells += "<td class='tdComment'>" + pupilsToAttendance[i].comment + "</td>"; }

        var row = $('<tr></tr>').addClass("trAttendance").html(cells);
        table.append(row);

    }

    var countCell = "<td>" + pupilsToAttendance.length + "</td><td>Are presence:</td><td id='tdcountOfPresence'>" + countOfPresencePupils +
        "</td><td>Number of missing:</td><td id='countOfMissing'>" + (pupilsToAttendance.length - countOfPresencePupils) + "</td>";
    row = $('<tr></tr>').html(countCell);
    table.append(row);
    $(".divTableContainer").append(table);
    $(".checkbox").change(function () {
        if (this.checked) {
            countOfPresencePupils++;
            $("#tdcountOfPresence").html(countOfPresencePupils);
            $("#countOfMissing").html(pupilsToAttendance.length - countOfPresencePupils);
        }
    });
}
function getListOfPupilsFromMyClass() {

    let divButtons = $(".divButtons");
    divButtons.append(btnClose);

    teacherID = localStorage['teacherID'];
    $(".spanResult").html("");
    $.get("api/Teacher/getMyClass?teacherID=" + teacherID, function (data, status) {
        if (status === "success") {

            if (data && data.length > 0) {

                pupilsFromMyClass = data;
                $("#className").append(pupilsFromMyClass[0].class.className);
                let table = $("<table></table>").addClass("pupilsListTable");
                let header = $("<tr><td></td><td>Name of Pupil</td><td>Birthday date</td><td>Phone</td><td>Address</td><td></td></tr>");
                table.append(header);

                for (var i = 0; i < pupilsFromMyClass.length; i++) {

                    let tr = $("<tr><td>" + (i + 1) + "</td><td>" + pupilsFromMyClass[i].name + " " + pupilsFromMyClass[i].surname +
                        "</td><td>" + pupilsFromMyClass[i].dateOfBirth + "</td><td>" + pupilsFromMyClass[i].phoneNumber +
                        "</td><td>" + pupilsFromMyClass[i].address + "</td><td><input type='button' id='btnSendMessage'" +
                        " value='Send message' onclick='showForm(" + i + ", \"messageForm\")'></input></td></tr>");
                    table.append(tr);
                }
                $(".divTableContainer").append(table);
                divButtons.append(btnClose);

            }
            else {
                $(".spanResult").html("<h1 align='center'>You don't have a class</h1>");/*.css('margin-left','40%' );*/
                $(".circle").hide();
                return;
            }
        }
        else {
            $(".spanResult").html("Oops! Something wrong in server side");
            return;
        }
    });

}
function showForm(i, formName) {

    makeForm("Enter your message:</br>", i, formName, function (value) {
        var message = {
            date: new Date(),
            teacherID: Number(localStorage['teacherID']),
            pupilID: pupilsFromMyClass[i].pupilID,
            userID: pupilsFromMyClass[i].userID,
            messageText: value
        };
        $.ajax({
            type: "PUT",
            url: "api/Teacher",
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify(message),
            success: function (result) {
                if (result) {
                    $(".spanResult").html("<h2>Your message is sended</h2>");
                    $(".divButtons").append(backButton);
                    return;
                }
                else {
                    $(".spanResult").html("<h2>Ooops! Something wrong!</h2>");
                    $(".divButtons").append(backButton);
                    return;
                }

            },
            failure: function (msg) {
                alert("error" + msg);
            }
        });

    });

}


function makeForm(text, i, formName, callback) {
    showCover();
    var form = document.getElementById("prompt-form");
    var container = document.getElementById("prompt-form-container");
    $("#prompt-message").html(text);
    if (formName === 'messageForm') {
        form.elements.text.value = '';
    }
    var value;
    function complete(value) {
        hideCover();
        container.style.display = 'none';
        document.onkeydown = null;
        if (value !== null)
            callback(value);
    }
    form.onsubmit = function () {

        if (formName === 'scheduleForm') {
            value = [];
            value[0] = form.elements.calendarFrom.value;
            value[1] = form.elements.calendarTo.value;

            if (value[0] === '' || value[1] === '')//ignore empty scheduleForm
                return false;
            var from = new Date(value[0]);
            var to = new Date(value[1]);

            if (from > to) {
                $(".spanResult").text("The period of dates is incorrect!");
                return false;
            }
        }
        if (formName === 'messageForm') {
            value = form.elements.text.value;
            if (value === '') { // ignore empty Messageform
                form.elements.text.focus();
                return false;
            }
        }
        complete(value);
        return false;
    };
    form.elements.cancel.onclick = function () {
        $(".spanResult").text("");
        complete(null);
    };
    container.style.display = 'block';
    if (formName === 'messageForm')
        form.elements.text.focus();
}



function showCover() {
    // creating a semitransparent DIV, shading the entire page and making the elements on it inaccessible
    //over this block will be placed a form to sending Message or form to getting Schedule 
    var coverDiv = document.createElement('div');
    coverDiv.id = 'cover-div';
    document.body.appendChild(coverDiv);
}

function hideCover() {
    document.body.removeChild(document.getElementById('cover-div'));

}

function renderFormToGetSchedule(formName, forTeacher) {
    var span = $(".spanResult").html("");
    makeForm("Please, choose the dates:</br><br />", 0, formName, function (value) {
        span.text("");
        $.get("api/Teacher/getSchedule?from=" + value[0] + "&to=" + value[1] + "&teacherID=" + localStorage['teacherID'] +
            "&forTeacher=" + forTeacher, function (data, status) {
                if (status === "success") {
                    if (!data) {
                        span.html("<h1>Response from server is not correct</h1>");
                        return;
                    }
                    if (data.length === 0) {
                        let text = "You don't have a lessons by Schedule in this period of dates";
                        if (!forTeacher)
                            text = "Where are no lessons in this period of dates";
                        span.html("<h1 align='center'>" + text + "</h1>");
                        backButton.css('margin-left', '50%');
                        span.append(backButton);
                        return;
                    }
                    schedule = data;
                    $(".divContainerScheduleButtons").hide();
                    renderShedule(forTeacher);

                }
                else {
                    span.html("<h1>Unsuccess</h1>");
                    return;
                }

            });
    });
}

function renderShedule(forTeacher) {

    var table = $("<table></table>").addClass('scheduleTable');
    var header = $("<tr></tr>");
    var header2 = $("<tr></tr>");

    if (forTeacher) {
        header.append("<td></td><td>My work Schedule</td><td></td><td></td>");
        header2.append("<td>Number of lesson</td><td>Class</td><td>Lesson</td><td>Comment</td>");
    }
    else {
        header.append("<td></td><td>Schedule of my Class</td><td></td><td></td>");
        header2.append("<td>Number of lesson</td><td>Teacher</td><td>Lesson</td><td>Comment</td>");
    }
    table.append(header);
    table.append(header2);

    let date = new Date(schedule[0].date);
    var trDate = $("<tr style='background-color:pink'><td></td><td>" + date.toDateString() + "</td><td></td><td></td></tr>");
    table.append(trDate);
    for (var i = 0; i < schedule.length; i++) {
        var tr = $("<tr></tr>");

        if (i > 0) {
            let nextDate = new Date(schedule[i].date);
            if (date.toDateString() !== nextDate.toDateString()) {
                trDate = $("<tr style='background-color:pink'><td></td><td>" + nextDate.toDateString() + "</td><td></td><td></td></tr>");
                table.append(trDate);
                date = nextDate;
            }
        }
        var cells = "</td><td>" + schedule[i].numberOfLesson + "</td>";
        if (forTeacher)
            cells += "<td>" + schedule[i].class.className + "</td>";
        else
            cells += "<td>" + schedule[i].teacher.name + " " + schedule[i].teacher.surname + "</td>";
        cells += "<td>" + schedule[i].lesson.lessonName + "</td><td>" + schedule[i].teachersComment + "</td></tr>";
        tr.append(cells);
        table.append(tr);

    }

    $(".divScheduleTableContainer").append(table);
    $(".divScheduleTableContainer").append(backButton);

}
function renderButtons() {
    $(".divContainerScheduleButtons").append(btnClose);
}
