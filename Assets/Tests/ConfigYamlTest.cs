using BuildingGen.Components;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VYaml.Serialization;

[TestFixture]
public class ConfigYamlTest
{
    private const string YAML_1 = @"
        version: ""0.3""

        parameters:
          xsize: 5
          ysize: 5
          breakForcePerLayer: 3000
          blockMass: 100
          blockHealth: 10
          physDamageMultiplier: 10
          hitboxPadding: 0.1
          breakTorque: 10000

        blockGroups:
          - id: layer1
            blocks:
              - id: b2_f
                pointType: Inside
              - id: b2_bc1
                pointType: Corner
              - id: b2_bw1
                pointType: Boundary

        buildings:
          - id: b1
            name: Futuristic Building
            parameters:
              generationAlgorithm: Grid
              linkSearchRadius: 0.3
            sections:
              - id: sc1
                position: [0, 0, 0]
                generationSettingsGrid:
                  size: [$xsize, $ysize, 1]
                  spacing: [1, 1, 1]
                blockGroupId: layer1
                parameters:
                  isStatic: true
        ";

    private static readonly YamlSerializerOptions yamlOptions = BuildingGenYamlFormatterResolver.Options;

    [Test]
    public void TestConfigFileDeserialize()
    {
        ConfigFile config = YamlSerializer.Deserialize<ConfigFile>( Encoding.UTF8.GetBytes( YAML_1 ), yamlOptions );

        Assert.IsNotNull( config );
        Assert.AreEqual( "0.3", config.Version );

        // Top-level parameters
        Assert.IsNotNull( config.Parameters );
        Assert.IsTrue( config.Parameters.ContainsKey( "xsize" ) );
        Assert.IsInstanceOf<Parameter<int>>( config.Parameters["xsize"] );
        Assert.AreEqual( 5, ((Parameter<int>)config.Parameters["xsize"]).Value );

        Assert.IsTrue( config.Parameters.ContainsKey( "hitboxPadding" ) );
        Assert.IsInstanceOf<Parameter<float>>( config.Parameters["hitboxPadding"] );
        Assert.AreEqual( 0.1f, ((Parameter<float>)config.Parameters["hitboxPadding"]).Value, 1e-6f );

        // Block groups
        Assert.IsNotNull( config.BlockGroups );
        Assert.AreEqual( 1, config.BlockGroups.Count );
        var bg0 = config.BlockGroups[0];
        Assert.AreEqual( "layer1", bg0.Id );
        Assert.IsNotNull( bg0.Blocks );
        Assert.AreEqual( 3, bg0.Blocks.Count );

        // Buildings
        Assert.IsNotNull( config.Buildings );
        Assert.AreEqual( 1, config.Buildings.Count );
        var b0 = config.Buildings[0];
        Assert.AreEqual( "b1", b0.Id );
        Assert.AreEqual( "Futuristic Building", b0.Name );

        // Building parameters
        Assert.IsNotNull( b0.Parameters );
        Assert.IsTrue( b0.Parameters.ContainsKey( "generationAlgorithm" ) );
        Assert.IsInstanceOf<Parameter<string>>( b0.Parameters["generationAlgorithm"] );
        Assert.AreEqual( "Grid", ((Parameter<string>)b0.Parameters["generationAlgorithm"]).Value );

        Assert.IsTrue( b0.Parameters.ContainsKey( "linkSearchRadius" ) );
        Assert.IsInstanceOf<Parameter<float>>( b0.Parameters["linkSearchRadius"] );
        Assert.AreEqual( 0.3f, ((Parameter<float>)b0.Parameters["linkSearchRadius"]).Value, 1e-6f );

        // Sections
        Assert.IsNotNull( b0.Sections );
        Assert.AreEqual( 1, b0.Sections.Count );
        var sec0 = b0.Sections[0];
        Assert.AreEqual( "sc1", sec0.Id );
        Assert.AreEqual( "layer1", sec0.BlockGroupId );

        // Section parameters
        Assert.IsNotNull( sec0.Parameters );
        Assert.IsTrue( sec0.Parameters.ContainsKey( "isStatic" ) );
        Assert.IsInstanceOf<Parameter<bool>>( sec0.Parameters["isStatic"] );
        Assert.AreEqual( true, ((Parameter<bool>)sec0.Parameters["isStatic"]).Value );

        // Evaluation context for resolving references to top-level parameters
        var topCtx = new EvaluationContext( null, config.Parameters );

        // Position
        var posObj = sec0.Position.Evaluate( topCtx );
        if (posObj is Vector3Int posI)
        {
            Assert.AreEqual( new Vector3Int( 0, 0, 0 ), posI );
        }
        else if (posObj is Vector3 posF)
        {
            Assert.AreEqual( new Vector3( 0f, 0f, 0f ), posF );
        }
        else
        {
            Assert.Fail( "Section.Position evaluated to unexpected type." );
        }

        // generationSettingsGrid.size should resolve $xsize and $ysize to top-level params -> (5,5,1)
        Assert.IsNotNull( sec0.GenerationSettingsGrid );
        var sizeObj = sec0.GenerationSettingsGrid.Size.Evaluate( topCtx );
        if (sizeObj is Vector3Int sizeI)
        {
            Assert.AreEqual( new Vector3Int( 5, 5, 1 ), sizeI );
        }
        else if (sizeObj is Vector3 sizeF)
        {
            Assert.AreEqual( new Vector3( 5f, 5f, 1f ), sizeF );
        }
        else
        {
            Assert.Fail( "GenerationSettingsGrid.Size evaluated to unexpected type." );
        }

        // spacing
        var spacingObj = sec0.GenerationSettingsGrid.Spacing.Evaluate( topCtx );
        if (spacingObj is Vector3Int spI)
        {
            Assert.AreEqual( new Vector3Int( 1, 1, 1 ), spI );
        }
        else if (spacingObj is Vector3 spF)
        {
            Assert.AreEqual( new Vector3( 1f, 1f, 1f ), spF );
        }
        else
        {
            Assert.Fail( "GenerationSettingsGrid.Spacing evaluated to unexpected type." );
        }
    }

    [Test]
    public void TestDeserializationTypes()
    {
        dynamic yaml = YamlSerializer.Deserialize<dynamic>( Encoding.UTF8.GetBytes( YAML_1 ) );

        // Базовые проверки корня
        Assert.IsTrue( yaml is object );
        Assert.AreEqual( typeof( Dictionary<object, object> ), ((object)yaml).GetType() );

        var dyaml = yaml as Dictionary<object, object>;
        Assert.IsNotNull( dyaml );
        Assert.IsTrue( dyaml.ContainsKey( "version" ) );
        Assert.IsTrue( dyaml.ContainsKey( "parameters" ) );
        Assert.IsTrue( dyaml.ContainsKey( "blockGroups" ) );
        Assert.IsTrue( dyaml.ContainsKey( "buildings" ) );

        // Проверка версии
        Assert.AreEqual( typeof( string ), dyaml["version"].GetType() );
        Assert.AreEqual( "0.3", dyaml["version"] );

        // Параметры — словарь
        var parameters = dyaml["parameters"] as Dictionary<object, object>;
        Assert.IsNotNull( parameters );
        Assert.IsTrue( parameters.ContainsKey( "xsize" ) );
        Assert.IsTrue( parameters.ContainsKey( "hitboxPadding" ) );

        // Числовые значения параметров
        var xsizeObj = parameters["xsize"];
        Assert.IsNotNull( xsizeObj );
        Assert.AreEqual( 5, Convert.ToInt32( xsizeObj ) );

        var hitboxObj = parameters["hitboxPadding"];
        Assert.IsNotNull( hitboxObj );
        Assert.AreEqual( 0.1, Convert.ToDouble( hitboxObj ), 1e-6 );

        // Группы блоков — список
        var blockGroups = dyaml["blockGroups"] as List<object>;
        Assert.IsNotNull( blockGroups );
        Assert.AreEqual( 1, blockGroups.Count );

        var bg0 = blockGroups[0] as Dictionary<object, object>;
        Assert.IsNotNull( bg0 );
        Assert.AreEqual( "layer1", bg0["id"] );

        var blocks = bg0["blocks"] as List<object>;
        Assert.IsNotNull( blocks );
        Assert.AreEqual( 3, blocks.Count );

        var firstBlock = blocks[0] as Dictionary<object, object>;
        Assert.IsNotNull( firstBlock );
        Assert.AreEqual( "b2_f", firstBlock["id"] );
        Assert.AreEqual( "Inside", firstBlock["pointType"] );

        // Здания и секции
        var buildings = dyaml["buildings"] as List<object>;
        Assert.IsNotNull( buildings );
        Assert.AreEqual( 1, buildings.Count );

        var b0 = buildings[0] as Dictionary<object, object>;
        Assert.IsNotNull( b0 );
        Assert.AreEqual( "b1", b0["id"] );
        Assert.AreEqual( "Futuristic Building", b0["name"] );

        var sections = b0["sections"] as List<object>;
        Assert.IsNotNull( sections );
        Assert.AreEqual( 1, sections.Count );

        var sec0 = sections[0] as Dictionary<object, object>;
        Assert.IsNotNull( sec0 );

        // Проверка generationSettingsGrid.size — это последовательность, где элементы могут быть ссылками или числами
        var genGrid = sec0["generationSettingsGrid"] as Dictionary<object, object>;
        Assert.IsNotNull( genGrid );
        Assert.IsTrue( genGrid.ContainsKey( "size" ) );

        var sizeSeq = genGrid["size"] as List<object>;
        Assert.IsNotNull( sizeSeq );
        // Первые элементы — строки с переменными ($xsize, $ysize), третий — число 1
        Assert.AreEqual( "$xsize", sizeSeq[0].ToString() );
        Assert.AreEqual( "$ysize", sizeSeq[1].ToString() );
        Assert.AreEqual( 1, Convert.ToInt32( sizeSeq[2] ) );

        // blockGroupId и параметры секции
        Assert.AreEqual( "layer1", sec0["blockGroupId"] );
        var secParams = sec0["parameters"] as Dictionary<object, object>;
        Assert.IsNotNull( secParams );
        Assert.IsTrue( secParams.ContainsKey( "isStatic" ) );
        Assert.AreEqual( true, Convert.ToBoolean( secParams["isStatic"] ) );
    }

    [Test]
    public void TestVector3Deserialization_Simple()
    {
        const string yamlVector = @"[1.0, 2.5, -3.3]";
        var result = YamlSerializer.Deserialize<Parameter>( Encoding.UTF8.GetBytes( yamlVector ), yamlOptions );

        EvaluationContext context = new( null, null );

        Assert.AreEqual( new Vector3( 1.0f, 2.5f, -3.3f ), (Vector3)result.Evaluate( context ) );
    }

    [Test]
    public void TestVector3Deserialization_PartialSimplify()
    {
        const string yamlVector = @"
            x: 1.0
            y: 2.5
            z: -3.3
            ";
        var result = YamlSerializer.Deserialize<Parameter>( Encoding.UTF8.GetBytes( yamlVector ), yamlOptions );

        EvaluationContext context = new( null, null );

        Assert.AreEqual( new Vector3( 1.0f, 2.5f, -3.3f ), (Vector3)result.Evaluate( context ) );
    }

    [Test]
    public void TestVector3Deserialization_Full()
    {
        const string yamlVector = @"
            vec3:
              x: 1.0
              y: 2.5
              z: -3.3
            ";
        var result = YamlSerializer.Deserialize<Parameter>( Encoding.UTF8.GetBytes( yamlVector ), yamlOptions );

        EvaluationContext context = new( null, null );

        Assert.AreEqual( new Vector3( 1.0f, 2.5f, -3.3f ), (Vector3)result.Evaluate( context ) );
    }

    [Test]
    public void TestBaseTypesDeserialization()
    {
        var vFloat = YamlSerializer.Deserialize<Parameter>( Encoding.UTF8.GetBytes( "1.0" ), yamlOptions );
        Assert.IsInstanceOf<Parameter<float>>( vFloat );
        Assert.AreEqual( 1.0f, ((Parameter<float>)vFloat).Value, 1e-6 );

        var vInt = YamlSerializer.Deserialize<Parameter>( Encoding.UTF8.GetBytes( "42" ), yamlOptions );
        Assert.IsInstanceOf<Parameter<int>>( vInt );
        Assert.AreEqual( 42, ((Parameter<int>)vInt).Value );

        var vString = YamlSerializer.Deserialize<Parameter>( Encoding.UTF8.GetBytes( "Hello, YAML!" ), yamlOptions );
        Assert.IsInstanceOf<Parameter<string>>( vString );
        Assert.AreEqual( "Hello, YAML!", ((Parameter<string>)vString).Value );

        var vRef = YamlSerializer.Deserialize<Parameter>( Encoding.UTF8.GetBytes( "$myParam" ), yamlOptions );
        Assert.IsInstanceOf<ParameterRef>( vRef );
        Assert.AreEqual( "myParam", ((ParameterRef)vRef).Reference );
    }

    // --- Новые тесты для поддерживаемых сложных и составных типов ---

    [Test]
    public void TestRandDeserialization_ShorthandAndNamed()
    {
        // Shorthand sequence -> ParameterRand (types inferred)
        const string yamlRandShort = @"[1, 10]";
        var randShort = YamlSerializer.Deserialize<Parameter>( Encoding.UTF8.GetBytes( yamlRandShort ), yamlOptions );
        Assert.IsInstanceOf<ParameterRand>( randShort );

        // Named wrapper rand_int -> forced integer subtype
        const string yamlRandNamed = @"
            rand_int:
              - 1
              - 10
            ";
        var randNamed = YamlSerializer.Deserialize<Parameter>( Encoding.UTF8.GetBytes( yamlRandNamed ), yamlOptions );
        Assert.IsInstanceOf<ParameterRand>( randNamed );
        Assert.AreEqual( SubType.Int, ((ParameterRand)randNamed).SubType );

        // Behavior: Evaluate produces number inside expected range (deterministic check: type and bounds)
        var ctx = new EvaluationContext( null, null );
        UnityEngine.Random.InitState( 12345 );
        var val = randShort.Evaluate( ctx );
        Assert.IsTrue( val is int || val is float );
        if (val is int)
        {
            int iv = (int)val;
            Assert.GreaterOrEqual( iv, 1 );
            Assert.LessOrEqual( iv, 10 );
        }
        else
        {
            float fv = (float)val;
            Assert.GreaterOrEqual( fv, 1f );
            Assert.Less( fv, 10f );
        }
    }

    [Test]
    public void TestOperationNodeDeserialization_FullNode()
    {
        const string yamlNode = @"
            node:
              operation: ""*""
              nodes:
                - ref: mult_var
                - node:
                    operation: ""+""
                    nodes:
                      - ref: add_var_1
                      - ref: add_var_2
            ";
        var param = YamlSerializer.Deserialize<Parameter>( Encoding.UTF8.GetBytes( yamlNode ), yamlOptions );
        Assert.IsInstanceOf<ParameterNode>( param );
        var node = (ParameterNode)param;
        Assert.AreEqual( OperationNode.MULTIPLY, node.Operation );
        Assert.AreEqual( 2, node.Operands.Count );

        Assert.IsInstanceOf<ParameterRef>( node.Operands[0] );
        Assert.AreEqual( "mult_var", ((ParameterRef)node.Operands[0]).Reference );

        Assert.IsInstanceOf<ParameterNode>( node.Operands[1] );
        var inner = (ParameterNode)node.Operands[1];
        Assert.AreEqual( OperationNode.ADD, inner.Operation );
        Assert.AreEqual( 2, inner.Operands.Count );
        Assert.IsInstanceOf<ParameterRef>( inner.Operands[0] );
        Assert.AreEqual( "add_var_1", ((ParameterRef)inner.Operands[0]).Reference );
    }

    [Test]
    public void TestOperationNodeDeserialization_ShortFormNested()
    {
        const string yaml = @"
            mult:
              - $mult_var
              - add:
                - $add_var_1
                - $add_var_2
            ";
        var param = YamlSerializer.Deserialize<Parameter>( Encoding.UTF8.GetBytes( yaml ), yamlOptions );
        Assert.IsInstanceOf<ParameterNode>( param );
        var node = (ParameterNode)param;
        Assert.AreEqual( OperationNode.MULTIPLY, node.Operation );
        Assert.AreEqual( 2, node.Operands.Count );

        Assert.IsInstanceOf<ParameterRef>( node.Operands[0] );
        Assert.AreEqual( "mult_var", ((ParameterRef)node.Operands[0]).Reference );

        Assert.IsInstanceOf<ParameterNode>( node.Operands[1] );
        var addNode = (ParameterNode)node.Operands[1];
        Assert.AreEqual( OperationNode.ADD, addNode.Operation );
        Assert.AreEqual( 2, addNode.Operands.Count );
        Assert.IsInstanceOf<ParameterRef>( addNode.Operands[0] );
        Assert.AreEqual( "add_var_1", ((ParameterRef)addNode.Operands[0]).Reference );
    }

    [Test]
    public void TestVec3Int_ArrayAndWrapper()
    {
        // Array of ints -> Vector3Int
        const string yamlArr = @"[1, 2, 3]";
        var pArr = YamlSerializer.Deserialize<Parameter>( Encoding.UTF8.GetBytes( yamlArr ), yamlOptions );
        var ctx = new EvaluationContext( null, null );
        var ev = pArr.Evaluate( ctx );
        Assert.IsInstanceOf<UnityEngine.Vector3Int>( ev );
        Assert.AreEqual( new Vector3Int( 1, 2, 3 ), (Vector3Int)ev );

        // vec3i wrapper -> Vector3Int
        const string yamlVec3i = @"
            vec3i:
              x: 1
              y: 2
              z: 3
            ";
        var pVec3i = YamlSerializer.Deserialize<Parameter>( Encoding.UTF8.GetBytes( yamlVec3i ), yamlOptions );
        var ev2 = pVec3i.Evaluate( ctx );
        Assert.IsInstanceOf<UnityEngine.Vector3Int>( ev2 );
        Assert.AreEqual( new Vector3Int( 1, 2, 3 ), (Vector3Int)ev2 );
    }
}