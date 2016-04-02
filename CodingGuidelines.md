

# General Guidelines #

---

| **Term** | **Description** |
|:---------|:----------------|
| Pascal case | The first letter in the identifier and the first letter of each subsequent concatenated word are capitalized. You can use Pascal case for identifiers of three or more characters.<br><i>Example: BackColor</i> <br>
<tr><td> Camel case </td><td> The first letter of an identifier is lowercase and the first letter of each subsequent concatenated word is capitalized.<br><i>Example: backColor</i> </td></tr>
<tr><td> Hungarian notation </td><td> Hungarian notation is a naming convention in computer programming, in which the name of a variable indicates its type or intended use. In other words, it is the practice of including a prefix in identifiers to encode some metadata about the parameter, such as the data type of the identifier.<br><i>Example: strBackColor (backColor is string type)</i> </td></tr></tbody></table>

<b>Naming</b>
<ul><li>Use abbreviations only when needed.<br>
</li><li>Do not use underscores. Except in constants.<br>
</li><li>Do not use acronyms that are not generally accepted in the computing field. Where appropriate, use well-known acronyms to replace lengthy phrase names.<br><i>E.g. use “OnButtonClick” instead of “OnBtnClick”.</i>
</li><li>Do not use namings that differ by only casing.<br><i>E.g. namespace ee.cummings; namespace Ee.Cummings;</i>
</li><li>Do not use Hungarian notation for naming properties and fields<br><i>E.g. string strBackColor</i></li></ul>

<b>Other Pointers</b>
<ul><li>If you need to hard code a value/string, declare them as constants.<br>
</li><li>Method name should explain what it does. Do not use misleading names.<br>
</li><li>Write comments wherever required. Good readable code will require very little comments. If all variables and method names are meaningful, that would already make the code fairly readable.</li></ul>

<h1>Class</h1>
<hr />
<b>Naming</b>
<ul><li>Use Pascal casing.<br><i>E.g. public class StudentRegistration</i></li></ul>

<h1>Interface</h1>
<hr />
<b>Naming</b>
<ul><li>Use Pascal casing.<br>
</li><li>Prefix with I in front of naming.<br><i>E.g. public interface IRegistration</i></li></ul>

<h1>Methods</h1>
<hr />
<b>Naming</b>
<ul><li>Use Pascal casing.<br><i>E.g. SaveFolderPaths()</i></li></ul>

<h1>Parameter</h1>
<hr />
<b>Naming</b>
<ul><li>Use Camel casing.<br><i>E.g. string studentName</i>