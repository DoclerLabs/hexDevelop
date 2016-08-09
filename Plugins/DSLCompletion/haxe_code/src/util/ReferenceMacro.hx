package util;

#if macro
import haxe.io.Path;
import haxe.macro.Compiler;
import haxe.macro.Context;
import haxe.macro.ExampleJSGenerator;
import haxe.macro.Expr.Position;
import haxe.macro.JSGenApi;
import haxe.macro.PositionTools;
import haxe.macro.TypeTools;
import haxe.macro.Type;
import sys.FileSystem;

using StringTools;

/**
 * Macro to print file and position of a type / field.
 * Also allows to complete code.
 * @author Christoph Otter
 */
class ReferenceMacro
{
	/**
	 * Prints the position (file and byte index) for the given input.
	 * @param	name can be a Class, Enum, TypeDef, Abstract, Function or Var
	 */
	macro public static function find (name : String) : Void
	{
		if (!findDeclaration (name)) {
			var lastDot = name.lastIndexOf (".");
			var pack = name.substring (0, lastDot);
			var field = name.substring (lastDot + 1);
			
			var result = findField (pack, field);
			
			if (!result) printError ("Type not found: " + name);
		}
		Sys.exit (0);
	}
	
	/**
	 * Prints the full path of the given file.
	 * @param	file a file path within one of the classpaths
	 */
	macro public static function getFile (file : String) : Void
	{
		var path = Context.resolvePath (file);
		printFile (path);
		Sys.exit (0);
	}
	
	/**
	 * Prints an array of possible options after the given code.
	 * @param	code a type / package path. It has to end with a dot
	 */
	macro public static function getCompletion (code : String) : Void
	{
		var list = new Array<String> ();
		
		if (!code.endsWith (".")) return;
		
		//TODO: consider using onGenerate
		
		code = code.substr (0, code.length - 1);
		
		var lst = listFields (code);
		if (lst != null) {
			for (field in lst) {
				list.push (field.name);
			}
		}
		else { //look for packages
			var cp = Context.getClassPath ();
			
			for (path in cp) {
				if (path == "") continue;
				
				var folder = Path.join ([path, code.replace (".", "/")]);
				
				if (FileSystem.exists (folder) && FileSystem.isDirectory (folder)) {
					var files = FileSystem.readDirectory (folder);
					for (file in files) {
						var split = file.split (".");
						if (split.length == 2 && split[1] == "hx" || split.length == 1) {
							if (list.indexOf (split[0]) == -1) { //check if already listed
								list.push (split[0]);
							}
						}
					}
				}
			}
		}
		
		list.sort (function (s0 : String, s1 : String) : Int {
			var a = s0.toLowerCase ();
			var b = s1.toLowerCase ();
			
			if (a == s0 && b != s1) return -1;
			if (a != s0 && b == s1) return 1;
			
			if (a < b) return -1;
			if (a > b) return 1;
			return 0;
		});
		
		printCompletion (list, "getCompletion " + code);
		Sys.exit (0);
	}
	
	/**
	 * Tries to search the given class for fields and prints them.
	 * @param	clazz the path of the class to be searched
	 * @param	statics whether you are looking for static of member fields
	 * @return true if the class was found, false otherwise (nothing is printed in the latter case)
	 */
	static function getFieldCompletion (clazz : String, statics = true) : Bool
	{
		var list = new Array<String> ();
		
		
		var fields = listFields (clazz, statics);
		
		if (fields != null) {
			printCompletion (list, "getFieldCompletion " + clazz + " " + statics);
		}
		
		return fields != null;
	}
	
	/**
	 * Returns an array of fields within clazz
	 * @param	statics denotes whether you are looking for static or member fields.
	 * @return	an array of fields if the class was found, null otherwise
	 */
	static function listFields (clazz : String, statics = true) : Array<Field>
	{
		if (clazz == "") return null;
		
		var list : Array<Field>;
		
		
		try {
			var type = Context.getType (clazz);
			
			//found a class / enum with specified name, so look for fields
			switch (type) {
				case TInst (cls, _):
					if (statics) {
						list = cls.get ().statics.get ();
					}
					else {
						list = cls.get ().fields.get ();
					}
				case TEnum (e, _):
					list = new Array<Field> ();
					for (c in e.get ().constructs) {
						list.push (c);
					}
				case TAbstract (t, _):
					//not possible
				case TType (t, _):
					switch (t.get ().type) {
						case TAnonymous (a): //only handle TAnonymous
							list = a.get ().fields;
						default:
					}
				default:
			}
		}
		catch (e : Dynamic) {}
		
		return list.length != 0 ? list : null;
	}
	
	/**
	 * Tries to find a field within a type and prints its position
	 * @param	t the type to search
	 * @param	name the field
	 * @return	true if it was found, false otherwise
	 */
	static function findField (t : String, name : String) : Bool
	{
		try {
			var	type = Context.getType (t);
			
			switch (type) {
				case TInst (_) | TEnum (_) | TType (_):
					var fields = listFields (t);
					if (fields == null) {
						fields = new Array<Field> ();
					}
					fields = fields.concat (listFields (t, false));
					
					for (field in fields) {
						if (field.name == name) {
							printPos (field, "findField " + t + " " + name);
							return true;
						}
					}
				case TAbstract (t, _):
					//no way to get pos info for abstract fields
					printPos (t.get (), "findField " + t + " " + name);
					return true;
				default:
			}
		}
		catch (e : Dynamic) {}
		
		return false;
	}
	
	/**
	 * Tries to find a type with the given name and prints out its position if it was found
	 * @return true if a type was found, false otherwise
	 */
	static function findDeclaration (name : String) : Bool
	{
		try {
			var type = Context.getType (name);
			var pos = getPositionObj (type);
			
			if (pos != null)
				printPos (pos, "findDeclaration " + name);
				return true;
			}
		catch (e : Dynamic) {
		}
		
		return false;
		
	}
	
	static function getPositionObj (type : Type) : PositionObject
	{
		switch (type) {
			case TEnum (e, _):
				return e.get ();
			case TInst (cls, _):
				return cls.get ();
			case TAbstract (a, _):
				return a.get ();
			case TType (t, _):
				return t.get ();
			default:
				return null;
		}
	}
	
	static function printCompletion (list : Array<String>, debug : String) : Void
	{
		//Sys.print ('"$debug"@');
		Sys.println ("ReferenceMacro " + Std.string (list));
	}
	
	static function printPos (p : PositionObject, debug : String) : Void
	{
		var pos = PositionTools.getInfos (p.pos);
		
		//Sys.print ('"$debug"@');
		Sys.println ("ReferenceMacro " + pos.file + ";" + pos.min);
	}
	
	static function printFile (file : String) : Void
	{
		Sys.println ("ReferenceMacro " + file);
	}
	
	static function printError (e : Dynamic) : Void
	{
		throw "ERROR: " + e;
	}
}

typedef PositionObject = {
	pos : Position
}

typedef Field = {
	> PositionObject,
	name : String
}
#end