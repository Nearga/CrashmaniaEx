const fs = require('fs');
const path = require('path');
const readline = require('readline');

const dumpFile = '/Users/vitaliivasylenko/Development/Unity/CrashmaniaEx/Research/deobfuscated/game4/classes_dump.txt';
const srcDir = '/Users/vitaliivasylenko/Development/Unity/CrashmaniaEx/Research/deobfuscated/game4/src';

async function processLineByLine() {
  const fileStream = fs.createReadStream(dumpFile);
  const rl = readline.createInterface({
    input: fileStream,
    crlfDelay: Infinity
  });

  const classes = {};
  let currentClass = null;

  for await (const line of rl) {
    // Parse class line
    // e.g. 0x00744628 [0x0043c244 - 0x0043c3e8]    420 ? class 11827 OSXStoreBindings
    // or with namespace in older dumps: 0x006a3d00 [0x002be5a2 - 0x002be5c0]     30 ? class 3173 AndroidOtp
    const classMatch = line.match(/class\s+\d+\s+([\w\.\<\>\`\_\[\]]+)/);
    if (classMatch) {
      const className = classMatch[1];
      currentClass = className;
      if (!classes[currentClass]) {
        classes[currentClass] = {
          name: currentClass,
          methods: []
        };
      }
      continue;
    }

    // Parse method line
    // e.g. 0x0043c244 ?   method   0 pvf  UnityEngine.Purchasing.OSXStoreBindings.SetUnityPurchasingCallback(1)
    // or: 0x002dd088 ?   method   8 pC   Crashmania.AccumulateToBalanceScript..ctor(0)
    const methodMatch = line.match(/method\s+\d+\s+(\w+)\s+([\w\.\<\>\`\_\[\]]+)\.([\w\.\<\>\`\_\[\]\:\.\<\>]+)\((\d+)\)/);
    if (methodMatch) {
      const modifier = methodMatch[1];
      const classPart = methodMatch[2];
      const methodName = methodMatch[3];
      const paramCount = parseInt(methodMatch[4], 10);

      const targetClass = classPart;
      if (!classes[targetClass]) {
        classes[targetClass] = {
          name: targetClass,
          methods: []
        };
      }
      classes[targetClass].methods.push({
        modifier,
        name: methodName,
        params: paramCount
      });
    }
  }

  console.log(`Parsed ${Object.keys(classes).length} classes. Reconstructing C# files...`);

  for (const className in classes) {
    const classInfo = classes[className];
    
    // Skip empty compiler generated modules
    if (className.startsWith('<Module>')) continue;
    
    // Determine Namespace and Class Name
    const parts = className.split('.');
    const simpleClassName = parts[parts.length - 1];
    const namespacePart = parts.slice(0, parts.length - 1).join('.');

    // Clean up generic symbols for valid C# names
    const cleanClassName = simpleClassName.replace(/\`/g, '_').replace(/[\<\>]/g, '');

    // Skip nested classes or weird symbols that won't compile easily at first pass
    if (cleanClassName.includes('+') || cleanClassName.includes('/') || cleanClassName.includes('$')) continue;

    let code = "using UnityEngine;\nusing System;\nusing System.Collections.Generic;\nusing UnityEngine.UI;\n\n";

    if (namespacePart) {
      code += `namespace ${namespacePart.replace(/\`/g, '_').replace(/[\<\>]/g, '')}\n{\n`;
    }

    // Check if it's likely an interface based on 'I' prefix and no constructor methods
    const hasConstructor = classInfo.methods.some(m => m.name === '.ctor' || m.name === '..ctor');
    const isInterface = cleanClassName.startsWith('I') && cleanClassName.length > 1 && charIsUpper(cleanClassName[1]) && !hasConstructor;

    const indent = namespacePart ? "\t" : "";

    if (isInterface) {
      code += `${indent}public interface ${cleanClassName}\n${indent}{\n`;
    } else {
      code += `${indent}public class ${cleanClassName} : MonoBehaviour\n${indent}{\n`;
    }

    // Generate Methods
    classInfo.methods.forEach(method => {
      // Clean generic names
      const cleanMethodName = method.name.replace(/\`/g, '_').replace(/[\<\>]/g, '').replace(/\:/g, '_');
      
      // Skip compiler generated lambdas/events
      if (cleanMethodName.startsWith('<') || cleanMethodName.includes('b__')) return;

      // Handle constructors
      if (cleanMethodName === '.ctor' || cleanMethodName === '..ctor') {
        if (!isInterface) {
          code += `${indent}\tpublic ${cleanClassName}() {}\n`;
        }
        return;
      }

      // Convert modifiers
      let csharpModifier = "public";
      if (method.modifier.includes('s')) csharpModifier += " static";
      if (method.modifier.includes('v') || method.modifier.includes('a')) csharpModifier += " virtual";

      // Prepare Parameters string
      const params = [];
      for (let i = 0; i < method.params; i++) {
        params.push(`object arg${i + 1}`);
      }
      const paramsStr = params.join(', ');

      if (isInterface) {
        code += `${indent}\tvoid ${cleanMethodName}(${paramsStr});\n`;
      } else {
        // Properties getters/setters helper
        if (cleanMethodName.startsWith('get_')) {
          const propName = cleanMethodName.substring(4);
          code += `${indent}\tpublic object ${propName} { get; }\n`;
        } else if (cleanMethodName.startsWith('set_')) {
          // Setter will be covered by getter or written simple
          return;
        } else {
          code += `${indent}\t${csharpModifier} object ${cleanMethodName}(${paramsStr}) { return null; }\n`;
        }
      }
    });

    code += `${indent}}\n`;
    if (namespacePart) {
      code += "}\n";
    }

    // Construct Output Directory structure
    const relPath = namespacePart.replace(/\./g, '/');
    const targetFolder = path.join(srcDir, relPath);
    fs.mkdirSync(targetFolder, { recursive: true });

    const targetFile = path.join(targetFolder, `${cleanClassName}.cs`);
    fs.writeFileSync(targetFile, code);
  }

  console.log("C# Class Reconstruction Completed successfully!");
}

function charIsUpper(c) {
  return c === c.toUpperCase() && c !== c.toLowerCase();
}

processLineByLine();
