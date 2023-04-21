using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var studentsQuery = from temp_course in db.Courses
                                join temp_classes in db.Classes
                                on temp_course.CourseNum equals temp_classes.CourseNum
                                into cc_pairs
                                from cc_pair in cc_pairs.DefaultIfEmpty()
                                join temp_enrolled in db.Enrolleds
                                on cc_pair.ClassNum equals temp_enrolled.ClassNum
                                into cce_pairs
                                from cce_pair in cce_pairs.DefaultIfEmpty()
                                join temp_students in db.Students
                                on cce_pair.UId equals temp_students.UId
                                into cces_pairs
                                from student_table in cces_pairs.DefaultIfEmpty()
                                where temp_course.Subject.Equals(subject) &&
                                temp_course.Num == num &&
                                cc_pair.Semester.Equals(season) &&
                                cc_pair.Year == year
                                select new
                                {
                                    fname = student_table.FName,
                                    lname = student_table.LName,
                                    uid = student_table.UId,
                                    dob = student_table.Dob,
                                    grade = cce_pair.Grade
                                };

                                 
            return Json(studentsQuery.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {

            if (category == null)
            {
                var noneAssignCatQuery = from temp_course in db.Courses
                                         join temp_class in db.Classes
                                         on new { A = temp_course.CourseNum, B = season, C = (uint)year } equals new { A = temp_class.CourseNum, B = temp_class.Semester, C = temp_class.Year }
                                         into course_class
                                         from cc_pair in course_class.DefaultIfEmpty()
                                         join temp_assignCat in db.AssignmentCategories
                                         on new { X = cc_pair.ClassNum } equals new { X = temp_assignCat.ClassNum }
                                         into assigncat_class
                                         from cca_pair in assigncat_class.DefaultIfEmpty()
                                         join temp_assign in db.Assignments
                                         on new { Z = cca_pair.CategoryNum } equals new { Z = temp_assign.CategoryNum }
                                         into ccaa_pair
                                         from contentTable in ccaa_pair.DefaultIfEmpty()
                                         where subject.Equals(temp_course.Subject) && num == temp_course.Num && contentTable != null
                                         select new
                                         {
                                             aname = contentTable != null ? contentTable.Name : "",
                                             //cname due
                                             cname = cca_pair.Name != null ? cca_pair.Name : "",
                                             due = contentTable != null ? contentTable.DueDate : new DateTime(),
                                            submissions = contentTable != null ? (from temp_submission in db.Submissions
                                                        where temp_submission.AssignmentNum == contentTable.AssignmentNum
                                                        select temp_submission.UId).Count() : 0
                                     };
                return Json(noneAssignCatQuery.ToArray());
            }

            var assignCatQuery = from temp_course in db.Courses
                                 join temp_class in db.Classes
                                 on new { A = temp_course.CourseNum, B = season, C = (uint)year } equals new { A = temp_class.CourseNum, B = temp_class.Semester, C = temp_class.Year }
                                 into course_class
                                 from cc_pair in course_class.DefaultIfEmpty()
                                 join temp_assignCat in db.AssignmentCategories
                                 on new { X = cc_pair.ClassNum, Y = category } equals new { X = temp_assignCat.ClassNum, Y = temp_assignCat.Name }
                                 into assigncat_class
                                 from cca_pair in assigncat_class.DefaultIfEmpty()
                                 join temp_assign in db.Assignments
                                 on new { Z = cca_pair.CategoryNum } equals new { Z = temp_assign.CategoryNum }
                                 into ccaa_pair
                                 from contentTable in ccaa_pair.DefaultIfEmpty()

                                 where subject.Equals(temp_course.Subject) && num == temp_course.Num && contentTable != null
                                 select new
                                 {
                                     aname = contentTable.Name,
                                     //cname due
                                     cname = cca_pair.Name,
                                     due = contentTable.DueDate,
                                     submissions = (from temp_submission in db.Submissions
                                                    where temp_submission.AssignmentNum == contentTable.AssignmentNum
                                                    select temp_submission).Count()
                                 };

            
            return Json(assignCatQuery.ToArray());
            //working
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var assignCatQuery = from temp_course in db.Courses
                                 join temp_class in db.Classes
                                 on new { A = temp_course.CourseNum, B = season, C = (uint)year } equals new { A = temp_class.CourseNum, B = temp_class.Semester, C = temp_class.Year }
                                 into course_class
                                 from cc_pair in course_class.DefaultIfEmpty()
                                 join temp_assignCat in db.AssignmentCategories
                                 on new { X = cc_pair.ClassNum } equals new { X = temp_assignCat.ClassNum }
                                 into assigncat_class
                                 from cca_pair in assigncat_class.DefaultIfEmpty()
                                 where cca_pair != null && temp_course.Subject.Equals(subject) && temp_course.Num == num
                                 select new
                                 {
                                     name = cca_pair.Name,
                                     weight = cca_pair.Weight
                                 };
            return Json(assignCatQuery.ToArray());
            //working
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            var classNumQuery = from temp_courses in db.Courses
                                join temp_classes in db.Classes
                                on temp_courses.CourseNum equals temp_classes.CourseNum
                                into cc_pairs
                                from cc_pair in cc_pairs.DefaultIfEmpty()
                                where subject.Equals(temp_courses.Subject) && num == temp_courses.Num
                                && cc_pair.Year == year && cc_pair.Semester.Equals(season)
                                select cc_pair.ClassNum;
            uint classNum = classNumQuery.First();

            var check = from temp_assignCat in db.AssignmentCategories
                        where temp_assignCat.Name.Equals(category) &&
                        temp_assignCat.ClassNum == classNum
                        select temp_assignCat;

            if( check.Count() > 0 )
            {
                return Json(new { success = false });
            }
            else
            {
                AssignmentCategory assignCat = new AssignmentCategory();

                assignCat.Name = category;
                assignCat.ClassNum = classNum;
                assignCat.Weight = (uint)catweight;
                db.AssignmentCategories.Add(assignCat);
            }

            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }

        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var assignCatNumQuery = from temp_course in db.Courses
                                    join temp_class in db.Classes
                                    on new { A = temp_course.CourseNum, B = season, C = (uint)year } equals new { A = temp_class.CourseNum, B = temp_class.Semester, C = temp_class.Year }
                                    into course_class
                                    from cc_pair in course_class.DefaultIfEmpty()
                                    join temp_assignCat in db.AssignmentCategories
                                    on new { X = cc_pair.ClassNum, Y = category } equals new { X = temp_assignCat.ClassNum, Y = temp_assignCat.Name }
                                    into assigncat_class
                                    from cca_pair in assigncat_class.DefaultIfEmpty()
                                    where subject.Equals(temp_course.Subject) && num == temp_course.Num
                                    && cc_pair.Year == year && cc_pair.Semester.Equals(season)
                                    select new
                                    {
                                        catNum = cca_pair.CategoryNum,
                                        classNum = cca_pair.ClassNum
                                    };
            uint catNum = assignCatNumQuery.First().catNum;
            uint classNum = assignCatNumQuery.First().classNum;
            Assignment assignment = new Assignment();

            assignment.Name = asgname;
            assignment.MaxPoint = (uint)asgpoints;
            assignment.DueDate = asgdue;
            assignment.Contents = asgcontents;
            assignment.CategoryNum = catNum;

            db.Assignments.Add(assignment);


            List<string> studentList = (from temp_course in db.Courses
                               join temp_class in db.Classes
                               on new { A = temp_course.CourseNum, B = season, C = (uint)year } equals new { A = temp_class.CourseNum, B = temp_class.Semester, C = temp_class.Year }
                               into course_class
                               from cc_pair in course_class.DefaultIfEmpty()
                               join temp_enrolled in db.Enrolleds
                               on cc_pair.ClassNum equals temp_enrolled.ClassNum
                               where subject.Equals(temp_course.Subject) && num == temp_course.Num
                               && cc_pair.Year == year && cc_pair.Semester.Equals(season) select temp_enrolled.UId).ToList();
              
            try
            {
                db.SaveChanges();

                updateGrade(studentList, classNum);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var submissionQuery = from temp_course in db.Courses
                                 join temp_class in db.Classes
                                 on new { A = temp_course.CourseNum, B = season, C = (uint)year } equals new { A = temp_class.CourseNum, B = temp_class.Semester, C = temp_class.Year }
                                 into course_class
                                 from cc_pair in course_class.DefaultIfEmpty()
                                 join temp_assignCat in db.AssignmentCategories
                                 on new { X = cc_pair.ClassNum, Y = category } equals new { X = temp_assignCat.ClassNum, Y = temp_assignCat.Name }
                                 into assigncat_class
                                 from cca_pair in assigncat_class.DefaultIfEmpty()
                                 join temp_assign in db.Assignments
                                 on new { Z = cca_pair.CategoryNum, V = asgname } equals new { Z = temp_assign.CategoryNum, V = temp_assign.Name }
                                 into ccaa_pair
                                 from contentTable in ccaa_pair.DefaultIfEmpty()
                                 join temp_submission in db.Submissions
                                 on contentTable.AssignmentNum equals temp_submission.AssignmentNum
                                 into ccaas_pairs
                                 from ccaas_pair in ccaas_pairs.DefaultIfEmpty()
                                 join temp_student in db.Students
                                 on ccaas_pair.UId equals temp_student.UId
                                 into ccaass_pairs
                                 from student_table in ccaass_pairs.DefaultIfEmpty()
                                  where subject.Equals(temp_course.Subject) && num == temp_course.Num && student_table != null
                                 select new
                                 {
                                     fname = student_table.FName,
                                     lname = student_table.LName,
                                     uid = student_table.UId,
                                     time = ccaas_pair.Time,
                                     score = ccaas_pair.Score
                                 };
            return Json(submissionQuery.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var submissionQuery = from temp_course in db.Courses
                                  join temp_class in db.Classes
                                  on new { A = temp_course.CourseNum, B = season, C = (uint)year } equals new { A = temp_class.CourseNum, B = temp_class.Semester, C = temp_class.Year }
                                  into course_class
                                  from cc_pair in course_class.DefaultIfEmpty()
                                  join temp_assignCat in db.AssignmentCategories
                                  on new { X = cc_pair.ClassNum, Y = category } equals new { X = temp_assignCat.ClassNum, Y = temp_assignCat.Name }
                                  into assigncat_class
                                  from cca_pair in assigncat_class.DefaultIfEmpty()
                                  join temp_assign in db.Assignments
                                  on new { Z = cca_pair.CategoryNum, W = asgname } equals new { Z = temp_assign.CategoryNum, W = temp_assign.Name }
                                  into ccaa_pair
                                  from contentTable in ccaa_pair.DefaultIfEmpty()
                                  join temp_submission in db.Submissions
                                  on new { U = contentTable.AssignmentNum, S = uid } equals new { U = temp_submission.AssignmentNum, S = temp_submission.UId }
                                  into submission_class
                                  from final_table in submission_class.DefaultIfEmpty()
                                  where subject.Equals(temp_course.Subject) && num == temp_course.Num
                                  select final_table;

            var classNumQuery = from temp_course in db.Courses
                                join temp_class in db.Classes
                                on new { A = temp_course.CourseNum, B = season, C = (uint)year } equals new { A = temp_class.CourseNum, B = temp_class.Semester, C = temp_class.Year }
                                into course_class
                                from cc_pair in course_class.DefaultIfEmpty()
                                where subject.Equals(temp_course.Subject) && num == temp_course.Num
                                select cc_pair.ClassNum;
            





          submissionQuery.First().Score = (uint)score;
            List<string> studentsList = new List<string>();
            studentsList.Add(uid);
            try
            {
                db.SaveChanges();
                updateGrade(studentsList, classNumQuery.First());
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }


        }

        private void updateGrade(List<string> uid, uint classNum)
        {
            //Dictionary<uint, double> weightDic = new Dictionary<uint, double>();
            Dictionary<uint, uint> assignNum = new Dictionary<uint, uint>();

            Dictionary<uint, double> weightDic = (from temp_class in db.Classes
                               join temp_assignCat in db.AssignmentCategories
                               on temp_class.ClassNum equals temp_assignCat.ClassNum
                               into ca_pairs
                               from ca_pair in ca_pairs.DefaultIfEmpty()
                               where temp_class.ClassNum == classNum && ca_pair != null
                               select new
                               {
                                   weight = ca_pair.Weight,
                                   catNum = ca_pair.CategoryNum
                               }).ToDictionary(x => x.catNum, x => (double)x.weight);

            var assignQuery =from temp_class in db.Classes
                             join temp_assignCat in db.AssignmentCategories
                             on temp_class.ClassNum equals temp_assignCat.ClassNum
                             into ca_pairs
                             from ca_pair in ca_pairs.DefaultIfEmpty()
                             join temp_assignment in db.Assignments
                             on ca_pair.CategoryNum equals temp_assignment.CategoryNum
                             into caa_pairs
                             from caa_pair in caa_pairs.DefaultIfEmpty()
                             where temp_class.ClassNum == classNum && caa_pair != null
                             select new
                             {
                                 catNum = caa_pair.CategoryNum
                             };

            foreach(var assign in assignQuery)
            {
                if (assignNum.ContainsKey(assign.catNum))
                {
                    assignNum[assign.catNum] += 1;
                }
                else
                {
                    assignNum.Add(assign.catNum, 1);
                }
            }

            Dictionary<uint, double> realWeightDic = new Dictionary<uint, double>();

            double totalWeight = 0;
            foreach(uint x in weightDic.Keys)
            {
                if (assignNum.ContainsKey(x))
                {
                    totalWeight += weightDic[x];
                }
            }

            double weightFactor = 100.0 / totalWeight;
            foreach(var cateNum in weightDic.Keys)
            {
                weightDic[cateNum] *= weightFactor;
            }



            foreach (var id in uid)
            {
                var allAssignmentsQuery = from temp_classes in db.Classes
                                          join temp_assignCat in db.AssignmentCategories
                                          on temp_classes.ClassNum equals temp_assignCat.ClassNum
                                          into cc_pairs
                                          from cc_pair in cc_pairs.DefaultIfEmpty()
                                          join temp_assign in db.Assignments
                                          on cc_pair.CategoryNum equals temp_assign.CategoryNum
                                          into cca_pairs
                                          from cca_pair in cca_pairs.DefaultIfEmpty()
                                          join temp_submission in db.Submissions
                                          on cca_pair.AssignmentNum equals temp_submission.AssignmentNum
                                          into ccaas_pairs
                                          from submission_table in ccaas_pairs.DefaultIfEmpty()
                                          where submission_table.UId == id && cca_pair != null && temp_classes.ClassNum == classNum
                                          select new
                                          {
                                              catNumber = cca_pair.CategoryNum,
                                              percent = submission_table.Score == null ? 0 : submission_table.Score,
                                              maxScore = cca_pair.MaxPoint
                                          };
                double gradePercent = 0.0;

                foreach(var temp in allAssignmentsQuery)
                {
                    double tempGrade = temp.percent == null ? 0 : (double)temp.percent / (double)temp.maxScore; 
                    gradePercent += tempGrade * weightDic[temp.catNumber];
                }
                string letterGrade = "";
                if (gradePercent >= 93)
                {
                    letterGrade = "A";
                }
                else if (gradePercent>= 90)
                {
                    letterGrade = "A-";
                }
                else if (gradePercent >= 87)
                {
                    letterGrade = "B+";
                }
                else if (gradePercent >= 83)
                {
                    letterGrade = "B";
                }
                else if (gradePercent >= 80)
                {
                    letterGrade = "B-";
                }
                else if (gradePercent >= 77)
                {
                    letterGrade = "C+";
                }
                else if (gradePercent >= 73)
                {
                    letterGrade = "C";
                }

                else if (gradePercent >= 70)
                {
                    letterGrade = "C-";
                }

                else if (gradePercent >= 67)
                {
                    letterGrade = "D+";
                }

                else if (gradePercent >= 63)
                {
                    letterGrade = "D";
                }

                else if (gradePercent >= 60)
                {
                    letterGrade = "D-";
                }
                else
                {
                    letterGrade = "E";
                }

                var gradeUpdate = from temp_enrolled in db.Enrolleds
                                  where temp_enrolled.UId == id &&
                                  temp_enrolled.ClassNum == classNum
                                  select temp_enrolled;

                gradeUpdate.First().Grade = letterGrade;
            }

            try
            {
                db.SaveChanges();
            }
            catch
            {
            }
            //working
        }





        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var classesQuery = from temp_class in db.Classes
                               join temp_course in db.Courses
                               on temp_class.CourseNum equals temp_course.CourseNum
                               into cc_pairs
                               from cc_pair in cc_pairs.DefaultIfEmpty()    
                               where temp_class.ProfessorId == uid
                               select new
                               {
                                   subject = cc_pair.Subject,
                                   number = cc_pair.Num,
                                   name = cc_pair.Name,
                                   season = temp_class.Semester,
                                   year = temp_class.Year
                               };
            return Json(classesQuery.ToArray());
            //working
        }


        
        /*******End code to modify********/
    }
}

